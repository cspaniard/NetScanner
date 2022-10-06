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

let scanAndOutputNetwork (options : ArgumentOptions) =

    BlackListBroker.init options

    let processTask =
        task {

            try
                let scanNetworkStopwatch = Stopwatch.StartNew()

                let! deviceInfos =
                    IIpService.scanNetworkAsync
                        { PingTimeOut = TimeOut.create options.PingTimeOut
                          Retries = Retries.create options.Retries
                          ShowMacs = options.ShowMacs
                          ShowNames = options.ShowNames
                          NameLookUpTimeOut = TimeOut.create options.NameLookUpTimeOut
                          Network = IpNetwork.create options.Network
                          BlackListFileName = FileName.create options.BlackListFile }

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
    | :? Parsed as opts -> scanAndOutputNetwork opts.Value
    | :? NotParsed as notParsed -> IHelpService.showHelp <| ArgErrors notParsed.Errors |> exit
    | _ -> Exception("No debiéramos llegar aquí.") |> IExceptionService.outputException
with
| :? AggregateException as ae -> IHelpService.showHelp <| ExceptionErrors ae.InnerExceptions |> exit
| e -> IExceptionService.outputException e ; exit EXIT_CODE_EXCEPTION
