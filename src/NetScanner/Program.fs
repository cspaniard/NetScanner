open System
open System.ComponentModel.DataAnnotations
open System.Diagnostics
open System.Diagnostics.CodeAnalysis
open System.Threading.Tasks
open CommandLine

open Microsoft.FSharp.Core
open Model
open Model.Constants
open Model.Definitions

type private IIpService = DI.Services.NetworkDI.IIpService
type private IHelpService = DI.Services.HelpDI.IHelpService
type private IExceptionService = DI.Services.ExceptionsDI.IExceptionService
type private IMetricService = DI.Services.DebugDI.IMetricService

//----------------------------------------------------------------------------------------------------------------------
[<DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof<ArgumentOptions>)>]
let appInit (options : ArgumentOptions) =

    DI.Brokers.NetworkDI.IIpBroker.init (PingTimeOut.create options.PingTimeOut)
                                        (Retries.create options.Retries)
                                        (NameLookupTimeOut.create options.NameLookUpTimeOut)

    DI.Brokers.StorageDI.IMacBlackListBroker.init (FileName.create options.BlackListFileName)
    DI.Brokers.StorageDI.IIpBlackListBroker.init (FileName.create options.IpBlackListFileName)
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
let scanAndOutputNetwork (options : ArgumentOptions) =

    let processTask =
        task {

            try
                let scanNetworkStopwatch = Stopwatch.StartNew ()

                let! deviceInfos =
                    IIpService.scanNetworkAsync options.ShowMacs options.ShowNames (IpNetwork.create options.Network)

                scanNetworkStopwatch.Stop ()

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

    processTask.Wait ()
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
try
    let args = Environment.GetCommandLineArgs () |> Array.tail
    use parser = new Parser (fun o -> o.HelpWriter <- null)

    match parser.ParseArguments<ArgumentOptions> args with
    | Parsed as opts ->
             appInit opts.Value
             scanAndOutputNetwork opts.Value
    | NotParsed as notParsed ->
             notParsed.Errors
             |> ArgErrors
             |> IHelpService.showHelp
             |> exit

with
| :? AggregateException as ae -> IHelpService.showHelp <| ExceptionErrors ae.InnerExceptions |> exit
| :? ValidationException as ve -> IHelpService.showHelp <| ValidationError ve |> exit
| e -> IExceptionService.outputException e
       exit EXIT_CODE_EXCEPTION
//----------------------------------------------------------------------------------------------------------------------
