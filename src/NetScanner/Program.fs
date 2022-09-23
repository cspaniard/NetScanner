open System
open System.Threading.Tasks
open CommandLine

open Model

open type Infrastructure.DI.Services.NetworkDI.IIpService
open type Infrastructure.DI.Services.HelpDI.IIHelpTextService

let argv = Environment.GetCommandLineArgs() |> Array.tail

let scanAndOutputNetwork (options : ArgumentOptions) =

    let processTask =
        task {
            let timeOut = TimeOut.create options.TimeOut
            let retries = Retries.create options.Retries
            let network = IpNetwork.create options.Network

            let! ipInfosWithMacs = scanNetworkAsync timeOut retries options.ShowMac network

            ipInfosWithMacs
            |> outputNetworkIpsStatus options.ActiveOnly options.Separator options.ShowMac
        } :> Task

    processTask.Wait()

try
    let parser = new Parser(fun o -> o.HelpWriter <- null)

    match parser.ParseArguments<ArgumentOptions> argv with
    | :? Parsed<ArgumentOptions> as opts -> scanAndOutputNetwork opts.Value
    | :? NotParsed<ArgumentOptions> as notParsed -> showHelpText <| ArgErrors notParsed.Errors |> exit
    | _ -> Console.WriteLine "No debiéramos llegar aquí."
with
| :? AggregateException as ae -> showHelpText <| ExceptionErrors ae.InnerExceptions |> exit
| e -> Console.WriteLine $"Raro que lleguemos aquí, pero: {e.Message}"
