namespace Brokers.Network.Network

open System

type Broker () =

    //------------------------------------------------------------------------------------------------------------------
    static member outputDeviceInfoLines (deviceInfoLines : string[]) =

        deviceInfoLines
        |> Array.iter Console.WriteLine
    //------------------------------------------------------------------------------------------------------------------
