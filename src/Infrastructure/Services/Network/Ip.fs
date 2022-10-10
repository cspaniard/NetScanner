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

        let scanStatusesAsyncTry () =
            [|
               for i in 1..254 -> IpAddress.create $"%s{network.value}{i}"
                                  |> IIpBroker.getDeviceInfoForIpAsync
            |]
            |> Task.WhenAll

        backgroundTask {
            let! deviceInfoStatuses = scanStatusesAsyncTry ()

            return (DeviceInfoArray.OfArray deviceInfoStatuses)
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static let getMacInfosForActiveIpsAsyncTry (deviceInfoArray : DeviceInfoArray) =

        let getMacsForActiveIpsAsyncTry () =

            deviceInfoArray.value
            |> Array.map (fun di -> if di.Active
                                    then IIpBroker.getMacForIpAsync di.IpAddress
                                    else MacInfo (di.IpAddress, Mac.create "") |> Task.FromResult)
            |> Task.WhenAll

        backgroundTask {

            let! macsForActiveIps = getMacsForActiveIpsAsyncTry ()

            return (MacInfoArray.OfArray macsForActiveIps)
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static let getNameInfosForActiveIpsAsyncTry (deviceInfoArray : DeviceInfoArray) =

        let getNameInfosForActiveIpsAsyncTry () =

            deviceInfoArray.value
            |> Array.map (fun di -> if di.Active
                                    then IIpBroker.getNameInfoForIpAsyncTry di.IpAddress
                                    else NameInfo (di.IpAddress, "") |> Task.FromResult)
            |> Task.WhenAll

        backgroundTask {
            let! namesForActiveIps = getNameInfosForActiveIpsAsyncTry ()

            return (NameInfoArray.OfArray namesForActiveIps)
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static let getMacBlackListTry () =

        IBlackListBroker.getMacBlacklistTry ()
        |> Array.map (Mac.clean >> Mac.create)
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static let scanMacInfoAsync blackList deviceInfoArray =

        //--------------------------------------------------------------------------------------------------------------
        let filterBlackList (blackList : Mac[]) (deviceInfoArray : DeviceInfoArray) =

            if blackList.Length = 0 then
                deviceInfoArray
            else
                deviceInfoArray.value
                |> Array.filter (fun di -> blackList |> (not << Array.contains di.Mac))
                |> DeviceInfoArray.OfArray
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        let mergeInfos (deviceInfoArray : DeviceInfoArray) (macInfoArray : MacInfoArray) =

           (deviceInfoArray.value, macInfoArray.value)
           ||> Array.map2 (fun di (MacInfo (_, mac)) -> { di with Mac = mac })
           |> DeviceInfoArray.OfArray
        //--------------------------------------------------------------------------------------------------------------

        backgroundTask {

            let! activeMacInfos = deviceInfoArray |> getMacInfosForActiveIpsAsyncTry

            return activeMacInfos
                   |> mergeInfos deviceInfoArray
                   |> filterBlackList blackList
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static let scanNameInfoAsync deviceInfoArray =

        //--------------------------------------------------------------------------------------------------------------
        let mergeInfos (deviceInfoArray : DeviceInfoArray) (nameInfoArray : NameInfoArray) =

           (deviceInfoArray.value, nameInfoArray.value)
           ||> Array.map2 (fun di (NameInfo (_, name)) -> { di with Name = name })
           |> DeviceInfoArray.OfArray
        //--------------------------------------------------------------------------------------------------------------

        backgroundTask {

            let! activeNameInfos = deviceInfoArray |> getNameInfosForActiveIpsAsyncTry

            return activeNameInfos
                   |> mergeInfos deviceInfoArray
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member scanNetworkAsync showMacs showNames network =

        backgroundTask {

            let! deviceInfoArray = scanStatusAsyncTry network

            let blackList = getMacBlackListTry()

            let! deviceInfoArray = if showMacs || blackList.Length > 0
                                   then scanMacInfoAsync blackList deviceInfoArray
                                   else deviceInfoArray |> Task.FromResult

            let! deviceInfoArray = if showNames
                                   then scanNameInfoAsync deviceInfoArray
                                   else deviceInfoArray |> Task.FromResult

            return deviceInfoArray
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

        outputParams.DeviceInfos.value
        |> filterFun
        |> buildInfoLinesFun
        |> INetworkBroker.outputDeviceInfoLines
    //------------------------------------------------------------------------------------------------------------------
