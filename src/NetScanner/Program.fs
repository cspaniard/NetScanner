open System
open System.ComponentModel.DataAnnotations
open System.Diagnostics
open System.Diagnostics.CodeAnalysis
open CommandLine

open Microsoft.Extensions.DependencyInjection
open Microsoft.FSharp.Core
open Model
open Model.Constants
open Model.Definitions
open DI.Interfaces
open DI.Providers


//----------------------------------------------------------------------------------------------------------------------
// TODO: Evaluar si se puede cambiar a una clase para usar DI.
[<DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof<ArgumentOptions>)>]
let scanAndOutputNetwork (ipService : IIpService)
                         (metricsService : IMetricsService)
                         (options : ArgumentOptions) =

    let processTask =
        backgroundTask {
            let scanNetworkStopwatch = Stopwatch.StartNew ()

            let! deviceInfos =
                ipService.scanNetworkAsync options.ShowMacs options.ShowNames options.UseDns
                                           (options.Network |> IpNetwork.create)

            scanNetworkStopwatch.Stop ()

            if options.Debug then
                metricsService.outputScanNetworkTimeTry scanNetworkStopwatch

            deviceInfos
            |> ipService.outputDeviceInfos options.ActivesOnly options.Separator options.ShowMacs options.ShowNames
        }

    processTask.GetAwaiter().GetResult()
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
let parser = new Parser (fun o -> o.HelpWriter <- null)

let parserResult =
    Environment.GetCommandLineArgs () |> Array.tail
    |> parser.ParseArguments<ArgumentOptions>

let ServiceProvider = ServiceProviderBuild parserResult.Value

let helpTextService = ServiceProvider.GetRequiredService<IHelpTextService>()
let exceptionService = ServiceProvider.GetRequiredService<IExceptionService>()
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
try
    match parserResult with
    | Parsed as opts ->
             let ipService = ServiceProvider.GetRequiredService<IIpService>()
             let metricsService = ServiceProvider.GetRequiredService<IMetricsService>()

             scanAndOutputNetwork ipService metricsService opts.Value
    | NotParsed as notParsed ->
             notParsed.Errors
             |> ArgErrors
             |> helpTextService.showHelp
             |> exit

with
| :? ValidationException as ve -> helpTextService.showHelp <| ValidationError ve |> exit
| e -> exceptionService.outputException e
       exit EXIT_CODE_EXCEPTION
//----------------------------------------------------------------------------------------------------------------------
