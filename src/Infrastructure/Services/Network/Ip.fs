namespace Services.Network.Ip

open System.Text.RegularExpressions
open System.Threading.Tasks
open Services.Network.NetworkHelper
open Motsoft.Util

type private IIpBroker = Infrastructure.DI.Brokers.NetworkDI.IIpBroker
type private INetworkBroker = Infrastructure.DI.Brokers.NetworkDI.INetworkBroker

type Service () =

    //----------------------------------------------------------------------------------------------------
    static member getAllIpStatusInNetworkAsyncTry timeOutPerIp retriesPerIp network =

        [ for i in 1..254 -> IIpBroker.pingIpAsync timeOutPerIp retriesPerIp $"%s{network}{i}" ]
        |> Task.WhenAll
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member getNetworksListAsyncTry () =
        IIpBroker.getIpV4NetworkClassesAsyncTry ()
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member scanNetworkAsync timeOut retries network =

        backgroundTask {

            return!
                validateNetworkTry network
                |> Service.getAllIpStatusInNetworkAsyncTry timeOut retries
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member outputNetworkIpsStatus activeOnly separator ipsInfo =

        let filterFun = Array.filter snd
        let separator = Regex.Unescape(separator)

        ipsInfo
        |> (if activeOnly then filterFun else id)
        |> Array.map (fun (ip, status) -> $"%s{ip}%s{separator}%b{status}")
        |> INetworkBroker.printNetworkIpsStatus
    //----------------------------------------------------------------------------------------------------
