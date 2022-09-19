namespace Brokers.Network.Network

open System

type Broker () =

    //----------------------------------------------------------------------------------------------------
    static member printNetworkIpsStatus (ipsInfoLines : string[]) =

        ipsInfoLines
        |> Array.iter Console.WriteLine
    //----------------------------------------------------------------------------------------------------
