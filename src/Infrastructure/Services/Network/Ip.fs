namespace Services.Network.Ip

open System.Text.RegularExpressions
open System.Threading.Tasks
open Motsoft.Util

open Model

type private IIpBroker = DI.Brokers.NetworkDI.IIpBroker
type private INetworkBroker = DI.Brokers.NetworkDI.INetworkBroker
type private IBlackListBroker = DI.Brokers.StorageDI.IBlackListBroker

type Service () =

    //------------------------------------------------------------------------------------------------------------------
    static let scanStatusAsyncTry (network : IpNetwork) =

        [|
           for i in 1..254 -> IpAddress.create $"%s{network.value}{i}"
                              |> IIpBroker.getDeviceInfoStatusForIpAsync
        |]
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

        IBlackListBroker.getMacBlacklistTry ()
        |> Array.map (Mac.clean >> Mac.create)
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

            let! deviceInfos = scanStatusAsyncTry network

            let blackList = getMacBlackListTry()

            let! deviceInfos = if scanMacs || blackList.Length > 0
                               then scanMacInfoAsyncTry blackList deviceInfos
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
