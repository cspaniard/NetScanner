open System
open System.Threading.Tasks
open CommandLine
open NetScanner.Options
open NetScanner.HelpText
open NetScanner.Model


type private IIpService = Infrastructure.DI.Services.NetworkDI.IIpService


let argv = Environment.GetCommandLineArgs() |> Array.tail

let scanAndOutputNetwork (options : ArgumentOptions) =

    let processTask =
        task {
            let network = IpNetwork.create options.Network
            let! ipInfosWithMacs = IIpService.scanNetworkAsync options.TimeOut options.Retries
                                                               options.ShowMac network

            ipInfosWithMacs
            |> IIpService.outputNetworkIpsStatus options.ActiveOnly options.Separator options.ShowMac
        } :> Task

    processTask.Wait()

try
    let parser = new Parser(fun o -> o.HelpWriter <- null)

    match parser.ParseArguments<ArgumentOptions> argv with
    | :? Parsed<ArgumentOptions> as opts -> scanAndOutputNetwork opts.Value
    | :? NotParsed<ArgumentOptions> as err -> showHelpText err
    | _ -> Console.WriteLine "No debiéramos llegar aquí."
with e -> Console.WriteLine e.Message
