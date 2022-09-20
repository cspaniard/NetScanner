namespace Brokers.Network.Network

open System

type Broker () =

    //----------------------------------------------------------------------------------------------------
    static member printNetworkIpInfoLines (ipsInfoLines : string[]) =

        ipsInfoLines
        |> Array.iter Console.WriteLine
    //----------------------------------------------------------------------------------------------------
