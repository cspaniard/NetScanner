namespace Brokers.Network.Network

open System

type Broker () =

    //----------------------------------------------------------------------------------------------------
    static member outputNetworkIpInfoLines (ipsInfoLines : string[]) =

        ipsInfoLines
        |> Array.iter Console.WriteLine
    //----------------------------------------------------------------------------------------------------
