open System
open System.Threading.Tasks
open CommandLine

open Model

type private IIpService = Infrastructure.DI.Services.NetworkDI.IIpService
type private IHelpService = Infrastructure.DI.Services.HelpDI.IHelpService

let argv = Environment.GetCommandLineArgs() |> Array.tail

let scanAndOutputNetwork (options : ArgumentOptions) =

    let processTask =
        task {
            let timeOut = TimeOut.create options.TimeOut
            let retries = Retries.create options.Retries
            let network = IpNetwork.create options.Network

            let! ipInfosWithMacs = IIpService.scanNetworkAsync timeOut retries options.ShowMac network

            ipInfosWithMacs
            |> IIpService.outputNetworkIpsStatus options.ActiveOnly options.Separator options.ShowMac
        } :> Task

    processTask.Wait()

try
    let parser = new Parser(fun o -> o.HelpWriter <- null)

    match parser.ParseArguments<ArgumentOptions> argv with
    | :? Parsed as opts -> scanAndOutputNetwork opts.Value
    | :? NotParsed as notParsed -> IHelpService.showHelp <| ArgErrors notParsed.Errors |> exit
    | _ -> Console.WriteLine "No debiéramos llegar aquí."
with
| :? AggregateException as ae -> IHelpService.showHelp <| ExceptionErrors ae.InnerExceptions |> exit
| e -> Console.WriteLine $"Raro que lleguemos aquí, pero: {e.Message}"
