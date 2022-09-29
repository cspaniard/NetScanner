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
    static let getAllNamesForNetworkAsyncTry (nameLookUpTimeOut : TimeOut) (network : IpNetwork) =

        [|
            for i in 1..254 -> IpAddress.create $"%s{network.value}{i}"
                               |> IIpBroker.getNameInfoForIpAsyncTry nameLookUpTimeOut
        |]
        |> Task.WhenAll
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let ipInfosWithBlankMac (ipStatuses : IpStatusArray) =

        // Todo: Arreglar pasando los nombres.
        ipStatuses.value
        |> Array.map (fun (IpStatus.IpStatus (ipAddress, active)) -> DeviceInfo (ipAddress, active, "", "dev_name"))
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let deviceInfosWithMac (ipStatuses : IpStatusArray) (macInfos : MacInfoArray)
                                  (nameInfos : NameInfoArray) =

        let getIndexFromIp (ipAddress : IpAddress) = ((ipAddress.value |> split ".")[3] |> int) - 1

        let fullInfos = ipStatuses |> ipInfosWithBlankMac

        macInfos.value
        |> Array.iter
               (fun (MacInfo (ipAddress, macInfoMac)) ->
                    let idx = getIndexFromIp ipAddress
                    let (IpStatus.IpStatus (ipInfoAddress, ipInfoActive)) = ipStatuses.value[idx]
                    let (NameInfo.NameInfo (_, deviceName)) = nameInfos.value[idx]
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
                return ipInfosWithBlankMac ipStatuses
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member outputNetworkIpsStatus activeOnly separator showMac ipInfoMacs =

        let formatMac (mac : string) =

            if mac |> String.IsNullOrWhiteSpace
            then ""
            else
                mac.ToCharArray()
                |> Array.splitInto(mac.Length / 2)
                |> Array.map String
                |> join "-"
                |> toUpper

        let filterFun = Array.filter (fun (DeviceInfo (_, active,_ , _)) -> active)
        let separator = Regex.Unescape(separator)

        let ipInfoOnly =
            Array.map (fun (DeviceInfo (ipAddress, status, _, _)) ->
                           $"%s{ipAddress.value}{separator}%b{status}")

        let withMac =
            Array.map (fun (DeviceInfo (ipAddress, status, mac, deviceName)) ->
                           $"%s{ipAddress.value}{separator}%b{status}{separator}%s{formatMac mac}" +
                           $"{separator}{deviceName}")

        ipInfoMacs
        |> (if activeOnly then filterFun else id)
        |> (if showMac then withMac else ipInfoOnly)
        |> INetworkBroker.printNetworkIpInfoLines
    //----------------------------------------------------------------------------------------------------
