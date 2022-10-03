namespace Brokers.Network.Network

open System

type Broker () =

    //----------------------------------------------------------------------------------------------------
    static member outputNetworkInfoLines (ipsInfoLines : string[]) =

        ipsInfoLines
        |> Array.iter Console.WriteLine
    //----------------------------------------------------------------------------------------------------
