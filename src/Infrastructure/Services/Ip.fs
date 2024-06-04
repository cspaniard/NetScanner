namespace Services

open System
open System.Runtime.InteropServices
open System.Text.RegularExpressions
open System.Threading.Tasks

open Model
open DI.Interfaces

type IpService (IpBroker : IIpBroker, NetworkBroker : INetworkBroker,
                MacBlackListBroker : IMacBlacklistBroker, IpBlacklistBroker : IIpBlacklistBroker) =

    //------------------------------------------------------------------------------------------------------------------
    let scanStatusAsyncTry (network : IpNetwork) (ipBlackList : IpAddress array) =

        let removeSetFromSet s1 s2 = Set.difference s2 s1

        set [ for i in 1..254 -> IpAddress.create $"%s{network.value}{i}" ]
        |> removeSetFromSet (ipBlackList |> Set.ofArray)
        |> Set.toArray
        |> Array.map IpBroker.getDeviceInfoStatusForIpAsync
        |> Task.WhenAll
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    let getMacInfosForActiveIpsAsyncTry (deviceInfos : DeviceInfo[]) =

        deviceInfos
        |> Array.map (fun di -> if di.Active
                                then IpBroker.getMacForIpAsync di.IpAddress
                                else MacInfo (di.IpAddress, Mac.create "") |> Task.FromResult)
        |> Task.WhenAll
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    let getNameInfosForActiveIpsAsyncTry useDns (deviceInfos : DeviceInfo[]) =

        deviceInfos
        |> Array.map (fun di -> if di.Active
                                then IpBroker.getNameInfoForIpAsyncTry useDns di.IpAddress
                                else NameInfo (di.IpAddress, "") |> Task.FromResult)
        |> Task.WhenAll
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    let getMacBlackListAsyncTry () =

        backgroundTask {
            let! macBlackList = MacBlackListBroker.getMacBlacklistAsyncTry ()

            return
                macBlackList
                |> Array.map Mac.create
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    let getIpBlackListAsyncTry () =

        backgroundTask {
            let! ipBlackList = IpBlacklistBroker.getIpBlacklistAsyncTry ()

            let validBlackListIps = ResizeArray<IpAddress>()
            let exceptionList = ResizeArray<Exception>()

            for ipStringValue in ipBlackList do
                try
                    IpAddress.create ipStringValue
                    |> validBlackListIps.Add
                with e ->
                    exceptionList.Add e

            if exceptionList.Count > 0 then
                raise (AggregateException(exceptionList))

            return validBlackListIps.ToArray()
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    let scanMacInfoAsyncTry blackList deviceInfos =

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
                    return! IpBroker.getLocalMacInfoForIpAsyncTry ipAddress
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
    let scanNameInfoAsyncTry useDns deviceInfos =

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
    interface IIpService with

        //--------------------------------------------------------------------------------------------------------------
        member _.scanNetworkAsync scanMacs scanNames useDns network =

            backgroundTask {

                let! macBlackList = getMacBlackListAsyncTry()
                let! ipBlackList = getIpBlackListAsyncTry()

                let! deviceInfos = scanStatusAsyncTry network ipBlackList

                let! deviceInfos = if scanMacs || macBlackList.Length > 0
                                   then scanMacInfoAsyncTry macBlackList deviceInfos
                                   else deviceInfos |> Task.FromResult

                let! deviceInfos = if scanNames
                                   then scanNameInfoAsyncTry useDns deviceInfos
                                   else deviceInfos |> Task.FromResult

                return deviceInfos
            }
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        member _.outputDeviceInfos activesOnly separator showMacs showNames (deviceInfos : DeviceInfo[]) =

            let filterFun =
                if activesOnly
                then Array.filter _.Active
                else id

            let separator = Regex.Unescape separator

            let buildInfoLinesFun =
                Array.map (fun di -> $"%s{di.IpAddress.value}%s{separator}%b{di.Active}" +
                                     (if showMacs then $"%s{separator}%s{di.Mac.formatted}" else "") +
                                     (if showNames then $"%s{separator}%s{di.Name}" else ""))

            deviceInfos
            |> filterFun
            |> buildInfoLinesFun
            |> NetworkBroker.outputDeviceInfoLines
        //--------------------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------------
