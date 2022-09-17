
open System
open System.Text.RegularExpressions
open System.Threading.Tasks

open CommandLine
open NetScanner.Options
open Motsoft.Util

open NetScanner.Exceptions

type private IIpService = Infrastructure.DI.Services.NetworkDI.IIpService


let argv = Environment.GetCommandLineArgs()
           |> Array.skip 1

let scanNetwork (argOptions : ArgumentOptions) =

    let checkNetworkTry network =
        let networkParts = network |> split "."

        networkParts.Length = 3 |> failWithIfFalse BAD_OCTET_COUNT

        networkParts
        |> Array.iter (fun np ->
                           let result, value = Int32.TryParse np
                           result |> failWithIfFalse NETWORK_INVALID_CHARS
                           (value < 0 || value > 254) |> failWithIfTrue NETWORK_INVALID_VALUES)

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
