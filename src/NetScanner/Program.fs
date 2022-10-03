open System
open System.Threading.Tasks
open CommandLine

open Microsoft.FSharp.Core
open Model

type private IIpService = Infrastructure.DI.Services.NetworkDI.IIpService
type private IHelpService = Infrastructure.DI.Services.HelpDI.IHelpService
type private IExceptionService = Infrastructure.DI.Services.ExceptionsDI.IExceptionService

let argv = Environment.GetCommandLineArgs() |> Array.tail

let scanAndOutputNetwork (options : ArgumentOptions) =

    let processTask =
        task {
            let pingTimeOut = TimeOut.create options.PingTimeOut
            let retries = Retries.create options.Retries
            let network = IpNetwork.create options.Network
            let nameLookUpTimeOut = TimeOut.create options.NameLookUpTimeOut

            try
                let! deviceInfos = IIpService.scanNetworkAsync pingTimeOut retries options.ShowMacs
                                                               options.ShowNames nameLookUpTimeOut network

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
    let parser = new Parser(fun o -> o.HelpWriter <- null)

    match parser.ParseArguments<ArgumentOptions> argv with
    | :? Parsed as opts -> scanAndOutputNetwork opts.Value
    | :? NotParsed as notParsed -> IHelpService.showHelp <| ArgErrors notParsed.Errors |> exit
    | _ -> Exception("No debiéramos llegar aquí.") |> IExceptionService.outputException
with
| :? AggregateException as ae -> IHelpService.showHelp <| ExceptionErrors ae.InnerExceptions |> exit
| e -> Exception($"Raro que lleguemos aquí, pero: {e.Message}") |> IExceptionService.outputException
