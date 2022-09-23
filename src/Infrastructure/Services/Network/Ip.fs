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
    static let getAllIpStatusInNetworkAsyncTry timeOut retries (network : IpNetwork) =

        [| for i in 1..254 -> IpAddress.create $"%s{network.value}{i}"
                              |> IIpBroker.pingIpAsync timeOut retries
        |]
        |> Task.WhenAll
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let getMacsForActiveIpsAsyncTry (timeOut : TimeOut) ipInfos =

        Arp.LinuxPingTimeout <- TimeSpan.FromMilliseconds(timeOut.value)

        ipInfos
        |> Array.filter (fun (IpInfo (_, active)) -> active)
        |> Array.map (fun (IpInfo (ipAddress, _)) -> IIpBroker.getMacForIpAsync ipAddress)
        |> Task.WhenAll
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let ipInfosWithBlankMac ipInfos =

        ipInfos
        |> Array.map (fun (IpInfo (ipAddress, active)) -> IpInfoMac (ipAddress, active, ""))
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let ipInfosWithMac ipInfos macInfos =

        let getIndexFromIp (ipAddress : IpAddress) = ((ipAddress.value |> split ".")[3] |> int) - 1

        let fullInfos = ipInfos |> ipInfosWithBlankMac

        macInfos
        |> Array.iter
               (fun (MacInfo (ipAddress, macInfoMac)) ->
                    let idx = getIndexFromIp ipAddress
                    let (IpInfo (ipInfoIp, ipInfoActive)) = ipInfos[idx]
                    fullInfos[idx] <- IpInfoMac (ipInfoIp, ipInfoActive, macInfoMac))

        fullInfos
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member getNetworksListAsyncTry () =
        IIpBroker.getIpV4NetworkClassesAsyncTry ()
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member scanNetworkAsync timeOut retries showMac network =

        backgroundTask {

            let! ipInfos = getAllIpStatusInNetworkAsyncTry timeOut retries network

            if showMac then
                let! macInfos = ipInfos |> getMacsForActiveIpsAsyncTry timeOut

                return
                    ipInfosWithMac ipInfos macInfos
            else
                return
                    ipInfos |> ipInfosWithBlankMac
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

        let filterFun = Array.filter (fun (IpInfoMac (_, active, _)) -> active)
        let separator = Regex.Unescape(separator)

        let ipInfoOnly =
            Array.map (fun (IpInfoMac (ipAddress, status, _)) ->
                           $"%s{ipAddress.value}{separator}%b{status}")

        let withMac =
            Array.map (fun (IpInfoMac (ipAddress, status, mac)) ->
                           $"%s{ipAddress.value}{separator}%b{status}{separator}%s{formatMac mac}")

        ipInfoMacs
        |> (if activeOnly then filterFun else id)
        |> (if showMac then withMac else ipInfoOnly)
        |> INetworkBroker.printNetworkIpInfoLines
    //----------------------------------------------------------------------------------------------------
