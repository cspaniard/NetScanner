namespace Brokers

open System
open DI.Interfaces

type MetricsBroker () =

    interface IMetricsBroker with
        //--------------------------------------------------------------------------------------------------------------
        member _.outputMeasurementTry (elementName : string) (ms : int64) =

            Console.Error.WriteLine $"%s{elementName}: %i{ms}ms."
        //--------------------------------------------------------------------------------------------------------------
