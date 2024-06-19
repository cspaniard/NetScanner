module NetScanner.App

open System.Diagnostics
open DI.Interfaces
open Model

type MainApp (ipService : IIpService, metricsService : IMetricsService, options : ArgumentOptions) =

    //------------------------------------------------------------------------------------------------------------------
    interface IMainApp with
        member _.runTry () =

            let processTask =
                backgroundTask {
                    let scanNetworkStopwatch = Stopwatch.StartNew ()

                    let! deviceInfos =
                        ipService.scanNetworkAsyncTry options.ShowMacs options.ShowNames options.UseDns
                                                      (options.Network |> IpNetwork.create)

                    scanNetworkStopwatch.Stop ()

                    if options.Debug then
                        metricsService.outputScanNetworkTimeTry scanNetworkStopwatch

                    deviceInfos
                    |> ipService.outputDeviceInfosTry options.ActivesOnly options.Separator
                                                      options.ShowMacs options.ShowNames
                }

            processTask.GetAwaiter().GetResult()
    //------------------------------------------------------------------------------------------------------------------
