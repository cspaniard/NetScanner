
open System
open System.Text.RegularExpressions
open System.Threading.Tasks

open CommandLine
open NetScanner.Options
open Motsoft.Util

type private IIpService = Infrastructure.DI.Services.NetworkDI.IIpService


let argv = Environment.GetCommandLineArgs()
           |> Array.skip 1

let scanNetwork (argOptions : ArgumentOptions) =

    let checkNetworkTry network =
        let networkParts = network |> split "."

        networkParts.Length = 3 |> failWithIfFalse "Número incorrecto de octetos."

        networkParts
        |> Array.iter (fun np ->
                           let result, value = Int32.TryParse np
                           result |> failWithIfFalse "La red sólo puede contener números."
                           (value < 0 || value > 254) |> failWithIfTrue "Valores inválidos en la red.")

    let cleanNetwork (network : string) =
        network.Trim('.') + "."

    let filterFun = Array.filter snd

    let printIpsInfo ipsInfo =
        let separator = Regex.Unescape(argOptions.Separator)

        ipsInfo
        |> (if argOptions.ActiveOnly then filterFun else id)
        |> Array.iter (fun (ip, status) -> Console.WriteLine $"%s{ip}%s{separator}%b{status}")


    let scanTask =
        task {
            checkNetworkTry argOptions.Network

            let! ipsInfo = IIpService.getAllIpStatusInNetworkAsyncTry argOptions.TimeOut
                                                                      argOptions.Retries
                                                                      (cleanNetwork argOptions.Network)

            printIpsInfo ipsInfo
        } :> Task

    scanTask.Wait()

try
    match Parser.Default.ParseArguments<ArgumentOptions> argv with
    | :? Parsed<ArgumentOptions> as opts -> scanNetwork opts.Value
    | :? NotParsed<ArgumentOptions> -> ()
    | _ -> Console.WriteLine "No debiéramos llegar aquí."
with e -> Console.WriteLine e.Message
