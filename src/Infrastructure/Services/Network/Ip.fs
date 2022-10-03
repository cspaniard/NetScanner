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
    static let getMacsForActiveIpsAsyncTry (timeOut : TimeOut) (ipStatuses : IpStatusArray) =

        Arp.LinuxPingTimeout <- TimeSpan.FromMilliseconds(timeOut.value)

        ipStatuses.value
        |> Array.filter (fun (IpStatus.IpStatus (_, active)) -> active)
        |> Array.map (fun (IpStatus.IpStatus (ipAddress, _)) -> IIpBroker.getMacForIpAsync ipAddress)
        |> Task.WhenAll
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let getAllNamesForNetworkAsyncTry (timeOut : TimeOut) (network : IpNetwork) =

        [|
            for i in 1..254 -> IpAddress.create $"%s{network.value}{i}"
                               |> IIpBroker.getNameInfoForIpAsyncTry timeOut
        |]
        |> Task.WhenAll
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let ipInfosWithNoMac (ipStatuses : IpStatusArray) (nameInfos : NameInfoArray) =

        (ipStatuses.value, nameInfos.value)
        ||> Array.map2 (fun (IpStatus.IpStatus (ipAddress, active)) (NameInfo.NameInfo (_, name)) ->
                            DeviceInfo (ipAddress, active, Mac.create "", name))
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let deviceInfosWithMac (ipStatuses : IpStatusArray) (macInfos : MacInfoArray)
                                  (nameInfos : NameInfoArray) =

        let getIndexFromIp (ipAddress : IpAddress) = ((ipAddress.value |> split ".")[3] |> int) - 1

        let fullInfos = ipInfosWithNoMac ipStatuses nameInfos

        macInfos.value
        |> Array.iter
               (fun (MacInfo (ipAddress, macInfoMac)) ->
                    let idx = getIndexFromIp ipAddress
                    let (DeviceInfo(ipInfoAddress, ipInfoActive, _, deviceName)) = fullInfos[idx]

                    fullInfos[idx] <- DeviceInfo (ipInfoAddress, ipInfoActive, macInfoMac, deviceName))

        fullInfos
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member scanNetworkAsync pingTimeOut retries showMac nameLookupTimeOut network =

        backgroundTask {

            let ipStatusesTask = getAllIpInfosForNetworkAsyncTry pingTimeOut retries network
            let nameInfosTask = getAllNamesForNetworkAsyncTry nameLookupTimeOut network

            let! ipInfos = Task.WhenAll [ ipStatusesTask ; nameInfosTask ]

            let ipStatuses = ipInfos[0] |> IpStatusArray.OfIpInfoArray
            let nameInfos = ipInfos[1] |> NameInfoArray.OfIpInfoArray

            if showMac then
                let! macInfos = ipStatuses |> getMacsForActiveIpsAsyncTry pingTimeOut

                return deviceInfosWithMac ipStatuses (MacInfoArray.OfArray macInfos) nameInfos
            else
                return ipInfosWithNoMac ipStatuses nameInfos
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member outputNetworkIpInfos activeOnly separator showMac deviceInfos =

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
        |> (if showMac then buildInfoLinesWithMac () else buildInfoLinesNoMac ())
        |> INetworkBroker.outputNetworkIpInfoLines
    //----------------------------------------------------------------------------------------------------
