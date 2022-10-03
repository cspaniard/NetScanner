namespace Services.Network.Ip

open System
open System.Text.RegularExpressions
open System.Threading.Tasks
open ArpLookup
open Motsoft.Util

open Model

type private IIpBroker = Infrastructure.DI.Brokers.NetworkDI.IIpBroker
type private INetworkBroker = Infrastructure.DI.Brokers.NetworkDI.INetworkBroker

type Service () =

    //----------------------------------------------------------------------------------------------------
    static let getAllIpInfosForNetworkAsyncTry pingTimeOut retries (network : IpNetwork) =

        [|
           for i in 1..254 -> IpAddress.create $"%s{network.value}{i}"
                              |> IIpBroker.pingIpAsync pingTimeOut retries
        |]
        |> Task.WhenAll
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let getMacsForActiveIpsAsyncTry (timeOut : TimeOut) (deviceInfos : DeviceInfo[]) =

        Arp.LinuxPingTimeout <- TimeSpan.FromMilliseconds(timeOut.value)

        deviceInfos
        |> Array.map (fun (DeviceInfo (ipAddress, active, _, _)) ->
                          if active then
                              IIpBroker.getMacForIpAsync ipAddress
                          else
                              backgroundTask { return MacInfo (ipAddress, Mac.create "") })
        |> Task.WhenAll
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let getNamesForActiveIpsAsyncTry (timeOut : TimeOut) (deviceInfos : DeviceInfo[]) =

        deviceInfos
        |> Array.map(fun (DeviceInfo (ipAddress, active, _, _)) ->
                         if active then
                             IIpBroker.getNameInfoForIpAsyncTry timeOut ipAddress
                         else
                             backgroundTask { return NameInfo (ipAddress, "") })
        |> Task.WhenAll
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member scanNetworkAsync pingTimeOut retries showMac showNames nameLookupTimeOut network =

        //------------------------------------------------------------------------------------------------
        let buildDeviceArrayFromStatusArray (ipStatusArray : IpStatusArray) =

            ipStatusArray.value
            |> Array.map (fun (IpStatus (ipAddress, active)) ->
                              DeviceInfo (ipAddress, active, Mac.create "", ""))
        //------------------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------------------
        let addMacInfo (deviceInfos : DeviceInfo[]) =

            let mergeInfos deviceInfos (macInfoArray : MacInfoArray) =

               (deviceInfos, macInfoArray.value)
               ||> Array.map2 (fun (DeviceInfo (ipAddress, active, _, name)) (MacInfo (_, mac)) ->
                                   DeviceInfo (ipAddress, active, mac, name))


            backgroundTask {
                if showMac then
                    let! activeMacInfos = deviceInfos |> getMacsForActiveIpsAsyncTry pingTimeOut

                    return mergeInfos deviceInfos (MacInfoArray.OfArray activeMacInfos)
                else
                    return deviceInfos
            }
        //------------------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------------------
        let addNameInfo (deviceInfos : DeviceInfo[]) =

            let mergeInfos deviceInfos (nameInfoArray : NameInfoArray) =

               (deviceInfos, nameInfoArray.value)
               ||> Array.map2 (fun (DeviceInfo (ipAddress, active, mac, _)) (NameInfo (_, name)) ->
                                   DeviceInfo (ipAddress, active, mac, name))

            backgroundTask {
                if showNames then
                    let! activeNameInfos = deviceInfos |> getNamesForActiveIpsAsyncTry nameLookupTimeOut

                    return mergeInfos deviceInfos (NameInfoArray.OfArray activeNameInfos)
                else
                    return deviceInfos
            }
        //------------------------------------------------------------------------------------------------

        backgroundTask {

            let! ipStatuses = getAllIpInfosForNetworkAsyncTry pingTimeOut retries network
            let deviceArray = buildDeviceArrayFromStatusArray <| IpStatusArray.OfArray ipStatuses

            let! deviceArray = addMacInfo deviceArray
            let! deviceArray = addNameInfo deviceArray

            return deviceArray
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member outputNetworkIpInfos activeOnly separator showMacs deviceInfos =

        let filterFun = Array.filter (fun (DeviceInfo (_, active,_ , _)) -> active)
        let separator = Regex.Unescape(separator)

        let buildInfoLinesWithMac () =
            Array.map (fun (DeviceInfo (ipAddress, status, mac, deviceName)) ->
                           $"%s{ipAddress.value}{separator}%b{status}{separator}%s{mac.formatted}" +
                           $"{separator}{deviceName}")

        let buildInfoLinesNoMac () =
            Array.map (fun (DeviceInfo (ipAddress, status, _, deviceName)) ->
                           $"%s{ipAddress.value}{separator}%b{status}{separator}{deviceName}")

        deviceInfos
        |> (if activeOnly then filterFun else id)
        |> (if showMacs then buildInfoLinesWithMac () else buildInfoLinesNoMac ())
        |> INetworkBroker.outputNetworkIpInfoLines
    //----------------------------------------------------------------------------------------------------
