
open System

open System.Threading.Tasks
open CommandLine
open NetScanner.Options

open NetScanner
open NetScanner.NetworkHelper

type private IIpService = Infrastructure.DI.Services.NetworkDI.IIpService


let argv = Environment.GetCommandLineArgs() |> Array.tail

let scanNetwork (argOptions : ArgumentOptions) =

    let scanTask =
        task {
            validateNetworkTry argOptions.Network

            return! IIpService.getAllIpStatusInNetworkAsyncTry argOptions.TimeOut
                                                               argOptions.Retries
                                                               (cleanNetwork argOptions.Network)
        }

    scanTask.Result


try
    let parser = new Parser(fun o -> o.HelpWriter <- null)

    match parser.ParseArguments<ArgumentOptions> argv with
    | :? Parsed<ArgumentOptions> as opts ->
        scanNetwork opts.Value
        |> printNetworkIpsStatus opts.Value.ActiveOnly opts.Value.Separator
    | :? NotParsed<ArgumentOptions> as err -> showErrors err
    | _ -> Console.WriteLine "No debiéramos llegar aquí."
with e -> Console.WriteLine e.Message
