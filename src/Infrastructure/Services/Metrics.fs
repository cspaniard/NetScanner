namespace Services

open System
open System.Diagnostics
open DI.Interfaces


type MetricsService (metricsBroker : IMetricsBroker) =

    interface IMetricsService with
        //--------------------------------------------------------------------------------------------------------------
        member _.outputScanNetworkTimeTry (stopwatch : Stopwatch) =

            if stopwatch = null then raise (ArgumentNullException <| nameof stopwatch)

            metricsBroker.outputMeasurementTry "Escaneado" stopwatch.ElapsedMilliseconds
        //--------------------------------------------------------------------------------------------------------------
