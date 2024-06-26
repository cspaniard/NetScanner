namespace Brokers

open System
open DI.Interfaces

type NetworkBroker () =

    //------------------------------------------------------------------------------------------------------------------
    interface INetworkBroker with
        member _.outputDeviceInfoLinesTry (deviceInfoLines : string[]) =

            deviceInfoLines
            |> Array.iter Console.WriteLine
    //------------------------------------------------------------------------------------------------------------------
