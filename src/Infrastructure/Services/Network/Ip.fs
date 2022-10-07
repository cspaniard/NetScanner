namespace Services.Network.Ip

open System
open System.Text.RegularExpressions
open System.Threading.Tasks
open ArpLookup
open Motsoft.Util

open Model
open Model.Definitions

type private IIpBroker = DI.Brokers.NetworkDI.IIpBroker
type private INetworkBroker = DI.Brokers.NetworkDI.INetworkBroker
type private IBlackListBroker = DI.Brokers.StorageDI.IBlackListBroker

type Service () =

    //----------------------------------------------------------------------------------------------------
    static let scanAllIpsAsyncTry pingTimeOut retries (network : IpNetwork) =

        [|
           for i in 1..254 -> IpAddress.create $"%s{network.value}{i}"
                              |> IIpBroker.getDeviceInfoStatus pingTimeOut retries
        |]
        |> Task.WhenAll
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let getMacsForActiveIpsAsyncTry (timeOut : TimeOut) (deviceInfoArray : DeviceInfoArray) =

        Arp.LinuxPingTimeout <- TimeSpan.FromMilliseconds(timeOut.value)

        deviceInfoArray.value
        |> Array.map (fun (DeviceInfo (ipAddress, active, _, _)) ->
                          if active then
                              IIpBroker.getMacForIpAsync ipAddress
                          else
                              backgroundTask { return MacInfo (ipAddress, Mac.create "") })
        |> Task.WhenAll
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let getNameInfosForActiveIpsAsyncTry (timeOut : TimeOut) (deviceInfoArray : DeviceInfoArray) =

        deviceInfoArray.value
        |> Array.map(fun (DeviceInfo (ipAddress, active, _, _)) ->
                         if active then
                             IIpBroker.getNameInfoForIpAsyncTry timeOut ipAddress
                         else
                             backgroundTask { return NameInfo (ipAddress, "") })
        |> Task.WhenAll
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let getMacBlacklistTry () =

        IBlackListBroker.getMacBlacklistTry ()
        |> Array.map (Mac.clean >> Mac.create)
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let scanMacInfoAsync pingTimeOut showMacs deviceInfoArray =

        //------------------------------------------------------------------------------------------------
        let filterBlackList (blackList : Mac[]) (deviceInfoArray : DeviceInfoArray) =
            if blackList.Length = 0 then
                deviceInfoArray
            else
                deviceInfoArray.value
                |> Array.filter (fun (DeviceInfo (_, _, mac, _)) -> blackList |> (not << Array.contains mac))
                |> DeviceInfoArray.OfArray
        //------------------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------------------
        let mergeInfos (deviceInfoArray : DeviceInfoArray) (macInfoArray : MacInfoArray) =

           (deviceInfoArray.value, macInfoArray.value)
           ||> Array.map2 (fun (DeviceInfo (ipAddress, active, _, name)) (MacInfo (_, mac)) ->
                               DeviceInfo (ipAddress, active, mac, name))
           |> DeviceInfoArray.OfArray
        //------------------------------------------------------------------------------------------------

        backgroundTask {

            let blackList = getMacBlacklistTry ()

            if showMacs || blackList.Length > 0 then

                let! activeMacInfos = deviceInfoArray |> getMacsForActiveIpsAsyncTry pingTimeOut

                return activeMacInfos
                       |> MacInfoArray.OfArray
                       |> mergeInfos deviceInfoArray
                       |> filterBlackList blackList
            else
                return deviceInfoArray
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let scanNameInfoAsync nameLookUpTimeOut showNames deviceInfoArray =

        let mergeInfos (deviceInfoArray : DeviceInfoArray) (nameInfoArray : NameInfoArray) =

           (deviceInfoArray.value, nameInfoArray.value)
           ||> Array.map2 (fun (DeviceInfo (ipAddress, active, mac, _)) (NameInfo (_, name)) ->
                               DeviceInfo (ipAddress, active, mac, name))
           |> DeviceInfoArray.OfArray

        backgroundTask {
            if showNames then

                let! activeNameInfos = deviceInfoArray |> getNameInfosForActiveIpsAsyncTry nameLookUpTimeOut

                return mergeInfos deviceInfoArray (NameInfoArray.OfArray activeNameInfos)
            else
                return deviceInfoArray
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member scanNetworkAsync (scanParams : ScanNetworkParams) =

        backgroundTask {

            let! deviceInfos = scanAllIpsAsyncTry scanParams.PingTimeOut
                                                  scanParams.Retries
                                                  scanParams.Network

            let! deviceInfoArray = DeviceInfoArray.OfArray deviceInfos
                                   |> scanMacInfoAsync scanParams.PingTimeOut
                                                       scanParams.ShowMacs

            let! deviceInfoArray = deviceInfoArray
                                   |> scanNameInfoAsync scanParams.NameLookUpTimeOut
                                                        scanParams.ShowNames

            return deviceInfoArray
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member outputDeviceInfos (outputParams : OutputDeviceInfosParams) =

        let filterFun =
            if outputParams.ActivesOnly
            then Array.filter (fun (DeviceInfo (_, active,_ , _)) -> active)
            else id

        let separator = Regex.Unescape(outputParams.Separator)

        let buildInfoLinesFun =
            Array.map (fun (DeviceInfo (ipAddress, status, mac, deviceName)) ->
                           $"%s{ipAddress.value}%s{separator}%b{status}" +
                           (if outputParams.ShowMacs then $"%s{separator}%s{mac.formatted}" else "") +
                           (if outputParams.ShowNames then $"%s{separator}%s{deviceName}" else ""))

        outputParams.DeviceInfos.value
        |> filterFun
        |> buildInfoLinesFun
        |> INetworkBroker.outputDeviceInfoLines
    //----------------------------------------------------------------------------------------------------
