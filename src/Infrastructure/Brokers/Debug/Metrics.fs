namespace Brokers.Debug.Metrics

open System

type Broker () =

    //------------------------------------------------------------------------------------------------------------------
    static member outputMeasurementTry elementName (ms : int64) =

        Console.Error.WriteLine $"%s{elementName}: %i{ms}ms."
    //------------------------------------------------------------------------------------------------------------------
