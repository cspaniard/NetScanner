namespace Services

open System
open System.Diagnostics
open DI.Interfaces


type MetricsService (MetricsBroker : IMetricsBroker) =

    interface IMetricsService with
        //--------------------------------------------------------------------------------------------------------------
        member _.outputScanNetworkTimeTry (stopwatch : Stopwatch) =

            if stopwatch = null then raise (ArgumentNullException <| nameof stopwatch)

            MetricsBroker.outputMeasurementTry "Escaneado" stopwatch.ElapsedMilliseconds
        //--------------------------------------------------------------------------------------------------------------
