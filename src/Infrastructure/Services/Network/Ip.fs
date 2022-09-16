namespace Services.Network.Ip

open System.Threading.Tasks
open Motsoft.Util

type private IIpBroker = Infrastructure.DI.Brokers.NetworkDI.IIpBroker

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
