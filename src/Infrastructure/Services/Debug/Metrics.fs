namespace Services.Debug.Metrics

open System
open System.Diagnostics

type private IMetricsBroker = DI.Brokers.DebugDI.IMetricsBroker

type Service () =

    static member outputScanNetworkTimeTry (stopwatch : Stopwatch) =

        if stopwatch = null then raise (ArgumentNullException(nameof stopwatch))

        IMetricsBroker.outputMeasurementTry "Escaneado" stopwatch.ElapsedMilliseconds
