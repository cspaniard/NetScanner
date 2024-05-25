namespace Services.Network.Ip

open System.Runtime.InteropServices
open System.Text.RegularExpressions
open System.Threading.Tasks

open Model

type private IIpBroker = DI.Brokers.NetworkDI.IIpBroker
type private INetworkBroker = DI.Brokers.NetworkDI.INetworkBroker
type private IMacBlackListBroker = DI.Brokers.StorageDI.IMacBlackListBroker
type private IIpBlackListBroker = DI.Brokers.StorageDI.IIpBlackListBroker

type Service () =

    //------------------------------------------------------------------------------------------------------------------
    static let scanStatusAsyncTry (ipBlackList : IpAddress array) (network : IpNetwork) =

        let removeSetFromSet s1 s2 = Set.difference s2 s1

        set [ for i in 1..254 -> IpAddress.create $"%s{network.value}{i}" ]
        |> removeSetFromSet (ipBlackList |> Set.ofArray)
        |> Set.toArray
        |> Array.map IIpBroker.getDeviceInfoStatusForIpAsync
        |> Task.WhenAll
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static let getMacInfosForActiveIpsAsyncTry (deviceInfos : DeviceInfo[]) =

        deviceInfos
        |> Array.map (fun di -> if di.Active
                                then IIpBroker.getMacForIpAsync di.IpAddress
                                else MacInfo (di.IpAddress, Mac.create "") |> Task.FromResult)
        |> Task.WhenAll
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static let getNameInfosForActiveIpsAsyncTry useDns (deviceInfos : DeviceInfo[]) =

        deviceInfos
        |> Array.map (fun di -> if di.Active
                                then IIpBroker.getNameInfoForIpAsyncTry useDns di.IpAddress
                                else NameInfo (di.IpAddress, "") |> Task.FromResult)
        |> Task.WhenAll
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static let getMacBlackListTry () =

        IMacBlackListBroker.getMacBlacklistTry ()
        |> Array.map (Mac.clean >> Mac.create)
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static let getIpBlackListTry () =

        IIpBlackListBroker.getIpBlacklistTry ()
        |> Array.map IpAddress.create
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static let scanMacInfoAsyncTry blackList deviceInfos =

        //--------------------------------------------------------------------------------------------------------------
        let filterMacBlackList (macBlackList : Mac[]) (deviceInfos : DeviceInfo[]) =

            if macBlackList.Length = 0 then
                deviceInfos
            else
                deviceInfos
                |> Array.filter (fun di -> macBlackList |> (not << Array.contains di.Mac))
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        let mergeInfos (deviceInfos : DeviceInfo[]) (macInfos : MacInfo[]) =

           (deviceInfos, macInfos)
           ||> Array.map2 (fun di (MacInfo (_, mac)) -> { di with Mac = mac })
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        let getlocalMacInfoForIpAsync (ipAddress : IpAddress) =
            backgroundTask {
                try
                    return! IIpBroker.getLocalMacInfoForIpAsyncTry ipAddress
                with _ ->
                    return MacInfo (ipAddress, Mac.create "")
            }
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        let getLocalMacInfosAsync (macInfos : DeviceInfo[]) =
            backgroundTask {
                if RuntimeInformation.IsOSPlatform OSPlatform.Windows then
                    return! Task.FromResult [||]
                else
                    return!
                        macInfos
                        |> Array.filter (fun di -> di.Active && not di.Mac.hasValue)
                        |> Array.map (fun di -> getlocalMacInfoForIpAsync di.IpAddress)
                        |> Task.WhenAll
            }
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        let mergeLocalMacInfos (deviceInfos : DeviceInfo[]) (localMacInfos : MacInfo[]) =
            [|
                for deviceInfo in deviceInfos do

                    let macInfoOption =
                        localMacInfos
                        |> Array.tryFind (fun (MacInfo (ipAddress, _)) -> deviceInfo.IpAddress = ipAddress)

                    match macInfoOption with
                    | Some (MacInfo (_, mac)) -> yield { deviceInfo with Mac = mac }
                    | None -> yield deviceInfo
            |]
        //--------------------------------------------------------------------------------------------------------------

        backgroundTask {

            let! allMacInfos = getMacInfosForActiveIpsAsyncTry deviceInfos

            let cleanDeviceInfos =
                allMacInfos
                |> mergeInfos deviceInfos
                |> filterMacBlackList blackList

            let! localMacInfos = getLocalMacInfosAsync cleanDeviceInfos

            return
                localMacInfos
                |> mergeLocalMacInfos cleanDeviceInfos
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static let scanNameInfoAsyncTry useDns deviceInfos =

        //--------------------------------------------------------------------------------------------------------------
        let mergeInfos (deviceInfos : DeviceInfo[]) (nameInfos : NameInfo[]) =

           (deviceInfos, nameInfos)
           ||> Array.map2 (fun di (NameInfo (_, name)) -> { di with Name = name })
        //--------------------------------------------------------------------------------------------------------------

        backgroundTask {

            let! activeNameInfos = getNameInfosForActiveIpsAsyncTry useDns deviceInfos

            return activeNameInfos
                   |> mergeInfos deviceInfos
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member scanNetworkAsync scanMacs scanNames useDns network =

        backgroundTask {

            let macBlackList = getMacBlackListTry()
            let ipBlackList = getIpBlackListTry()

            let! deviceInfos = scanStatusAsyncTry ipBlackList network

            let! deviceInfos = if scanMacs || macBlackList.Length > 0
                               then scanMacInfoAsyncTry macBlackList deviceInfos
                               else deviceInfos |> Task.FromResult

            let! deviceInfos = if scanNames
                               then scanNameInfoAsyncTry useDns deviceInfos
                               else deviceInfos |> Task.FromResult

            return deviceInfos
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member outputDeviceInfos (outputParams : OutputDeviceInfosParams) (deviceInfos : DeviceInfo[]) =

        let filterFun =
            if outputParams.ActivesOnly
            then Array.filter _.Active
            else id

        let separator = Regex.Unescape outputParams.Separator

        let buildInfoLinesFun =
            Array.map (fun di -> $"%s{di.IpAddress.value}%s{separator}%b{di.Active}" +
                                 (if outputParams.ShowMacs then $"%s{separator}%s{di.Mac.formatted}" else "") +
                                 (if outputParams.ShowNames then $"%s{separator}%s{di.Name}" else ""))

        deviceInfos
        |> filterFun
        |> buildInfoLinesFun
        |> INetworkBroker.outputDeviceInfoLines
    //------------------------------------------------------------------------------------------------------------------
