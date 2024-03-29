namespace Services.Network.Ip

open System.Text.RegularExpressions
open System.Threading.Tasks
open Motsoft.Util

open Model

type private IIpBroker = DI.Brokers.NetworkDI.IIpBroker
type private INetworkBroker = DI.Brokers.NetworkDI.INetworkBroker
type private IMacBlackListBroker = DI.Brokers.StorageDI.IMacBlackListBroker
type private IIpBlackListBroker = DI.Brokers.StorageDI.IIpBlackListBroker

type Service () =

    //------------------------------------------------------------------------------------------------------------------
    static let scanStatusAsyncTry (ipBlackList : IpAddress array) (network : IpNetwork) =

        let blakListValues = ipBlackList |> Array.map (fun ip -> ip.value)

        [| for i in 1..254 -> IpAddress.create $"%s{network.value}{i}" |]
        |> Array.filter (fun ip -> blakListValues |> Array.contains ip.value = false)
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
    static let getNameInfosForActiveIpsAsyncTry (deviceInfos : DeviceInfo[]) =

        deviceInfos
        |> Array.map (fun di -> if di.Active
                                then IIpBroker.getNameInfoForIpAsyncTry di.IpAddress
                                else NameInfo (di.IpAddress, "") |> Task.FromResult)
        |> Task.WhenAll

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
        let filterBlackList (blackList : Mac[]) (deviceInfos : DeviceInfo[]) =

            if blackList.Length = 0 then
                deviceInfos
            else
                deviceInfos
                |> Array.filter (fun di -> blackList |> (not << Array.contains di.Mac))
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        let mergeInfos (deviceInfos : DeviceInfo[]) (macInfos : MacInfo[]) =

           (deviceInfos, macInfos)
           ||> Array.map2 (fun di (MacInfo (_, mac)) -> { di with Mac = mac })
        //--------------------------------------------------------------------------------------------------------------

        backgroundTask {

            let! activeMacInfos = getMacInfosForActiveIpsAsyncTry deviceInfos

            return activeMacInfos
                   |> mergeInfos deviceInfos
                   |> filterBlackList blackList
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static let scanNameInfoAsyncTry deviceInfos =

        //--------------------------------------------------------------------------------------------------------------
        let mergeInfos (deviceInfos : DeviceInfo[]) (nameInfos : NameInfo[]) =

           (deviceInfos, nameInfos)
           ||> Array.map2 (fun di (NameInfo (_, name)) -> { di with Name = name })
        //--------------------------------------------------------------------------------------------------------------

        backgroundTask {

            let! activeNameInfos = getNameInfosForActiveIpsAsyncTry deviceInfos

            return activeNameInfos
                   |> mergeInfos deviceInfos
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member scanNetworkAsync scanMacs scanNames network =

        backgroundTask {


            let macBlackList = getMacBlackListTry()
            let ipBlackList = getIpBlackListTry()

            let! deviceInfos = scanStatusAsyncTry ipBlackList network

            let! deviceInfos = if scanMacs || macBlackList.Length > 0
                               then scanMacInfoAsyncTry macBlackList deviceInfos
                               else deviceInfos |> Task.FromResult

            let! deviceInfos = if scanNames
                               then scanNameInfoAsyncTry deviceInfos
                               else deviceInfos |> Task.FromResult

            return deviceInfos
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member outputDeviceInfos (outputParams : OutputDeviceInfosParams) =

        let filterFun =
            if outputParams.ActivesOnly
            then Array.filter (fun di -> di.Active)
            else id

        let separator = Regex.Unescape outputParams.Separator

        let buildInfoLinesFun =
            Array.map (fun di -> $"%s{di.IpAddress.value}%s{separator}%b{di.Active}" +
                                 (if outputParams.ShowMacs then $"%s{separator}%s{di.Mac.formatted}" else "") +
                                 (if outputParams.ShowNames then $"%s{separator}%s{di.Name}" else ""))

        outputParams.DeviceInfos
        |> filterFun
        |> buildInfoLinesFun
        |> INetworkBroker.outputDeviceInfoLines
    //------------------------------------------------------------------------------------------------------------------
