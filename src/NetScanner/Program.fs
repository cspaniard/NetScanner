open System
open System.Diagnostics
open System.Threading.Tasks
open CommandLine

open Microsoft.FSharp.Core
open Model
open Model.Definitions
open Model.Constants

type private IIpService = DI.Services.NetworkDI.IIpService
type private IHelpService = DI.Services.HelpDI.IHelpService
type private IExceptionService = DI.Services.ExceptionsDI.IExceptionService
type private IMetricService = DI.Services.DebugDI.IMetricService

type private BlackListBroker = DI.Brokers.StorageDI.IBlackListBroker

let appInit (options : ArgumentOptions) =

    DI.Brokers.NetworkDI.IIpBroker.init (PingTimeOut.create options.PingTimeOut)
                                        (NameLookupTimeOut.create options.NameLookUpTimeOut)

    DI.Brokers.StorageDI.IBlackListBroker.init (FileName.create options.BlackListFile)


let scanAndOutputNetwork (options : ArgumentOptions) =

    let processTask =
        task {

            try
                let scanNetworkStopwatch = Stopwatch.StartNew()

                let! deviceInfos =
                    IIpService.scanNetworkAsync
                        options.ShowMacs options.ShowNames (IpNetwork.create options.Network)

                scanNetworkStopwatch.Stop()

                if options.Debug then
                    IMetricService.outputScanNetworkTimeTry scanNetworkStopwatch

                IIpService.outputDeviceInfos
                    { ActivesOnly = options.ActivesOnly
                      Separator = options.Separator
                      ShowMacs = options.ShowMacs
                      ShowNames = options.ShowNames
                      DeviceInfos = deviceInfos }

            with e -> IExceptionService.outputException e
        } :> Task

    processTask.Wait()

try
    let argv = Environment.GetCommandLineArgs() |> Array.tail
    let parser = new Parser(fun o -> o.HelpWriter <- null)

    match parser.ParseArguments<ArgumentOptions> argv with
    | :? Parsed as opts -> appInit opts.Value
                           scanAndOutputNetwork opts.Value
    | :? NotParsed as notParsed -> notParsed.Errors
                                   |> ArgErrors
                                   |> IHelpService.showHelp
                                   |> exit
    | _ -> Exception("No debiéramos llegar aquí.") |> IExceptionService.outputException
with
| :? AggregateException as ae -> IHelpService.showHelp <| ExceptionErrors ae.InnerExceptions |> exit
| e -> IExceptionService.outputException e ; exit EXIT_CODE_EXCEPTION
// TODO: ValidationException ?
