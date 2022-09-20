namespace Services.Network.Ip

open System
open System.Text.RegularExpressions
open System.Threading.Tasks
open ArpLookup
open Services.Network.NetworkHelper
open Motsoft.Util

type private IIpBroker = Infrastructure.DI.Brokers.NetworkDI.IIpBroker
type private INetworkBroker = Infrastructure.DI.Brokers.NetworkDI.INetworkBroker

type Service () =

    //----------------------------------------------------------------------------------------------------
    static let getAllIpStatusInNetworkAsyncTry timeOut retries network =

        [ for i in 1..254 -> IIpBroker.pingIpAsync timeOut retries $"%s{network}{i}" ]
        |> Task.WhenAll
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let getMacsForActiveIpsAsyncTry (timeOut : int) (ipInfos : (string * bool)[]) =

        Arp.LinuxPingTimeout <- TimeSpan.FromMilliseconds(timeOut)

        ipInfos
        |> Array.filter snd
        |> Array.map (fun (ip, _) -> IIpBroker.getMacForIpAsync ip)
        |> Task.WhenAll
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let ipInfosWithBlankMac (ipInfos : (string * bool)[]) =

        ipInfos
        |> Array.map (fun (ip, active) -> (ip, active, ""))
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let ipInfosWithMac (ipInfos : (string * bool)[]) (macInfos : (string * string)[]) =

        let getIndexFromIp (ip : string) = ((ip |> split ".")[3] |> int) - 1

        let fullInfos = ipInfos |> ipInfosWithBlankMac

        macInfos
        |> Array.iter
               (fun (ip, macInfoMac) ->
                    let idx = getIndexFromIp ip
                    let ipInfoIp, ipInfoActive = ipInfos[idx]
                    fullInfos[idx] <- (ipInfoIp, ipInfoActive, macInfoMac))

        fullInfos
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member getNetworksListAsyncTry () =
        IIpBroker.getIpV4NetworkClassesAsyncTry ()
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member scanNetworkAsync timeOut retries network showMac =

        backgroundTask {

            let network = validateNetworkTry network

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
    static member outputNetworkIpsStatus activeOnly separator showMac ipInfos =

        let formatMac (mac : string) =

            if mac |> String.IsNullOrWhiteSpace
            then ""
            else
                mac.ToCharArray()
                |> Array.splitInto(mac.Length / 2)
                |> Array.map String
                |> join "-"
                |> toUpper

        let filterFun = Array.filter (fun (_, active, _) -> active)
        let separator = Regex.Unescape(separator)

        let ipInfoOnly = Array.map (fun (ip, status, _) -> $"%s{ip}{separator}%b{status}")
        let withMac = Array.map (fun (ip, status, mac) ->
                                    $"%s{ip}{separator}%b{status}{separator}%s{formatMac mac}")

        ipInfos
        |> (if activeOnly then filterFun else id)
        |> (if showMac then withMac else ipInfoOnly)
        |> INetworkBroker.printNetworkIpInfoLines
    //----------------------------------------------------------------------------------------------------
