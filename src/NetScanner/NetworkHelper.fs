module NetScanner.NetworkHelper

open System
open System.Text.RegularExpressions

open NetScanner.Exceptions
open Motsoft.Util

let validateNetworkTry network =
    let networkParts = network |> split "."

    networkParts.Length = 3 |> failWithIfFalse BAD_OCTET_COUNT

    networkParts
    |> Array.iter (fun np ->
                       let result, value = Int32.TryParse np
                       result |> failWithIfFalse NETWORK_INVALID_CHARS
                       (value < 0 || value > 254) |> failWithIfTrue NETWORK_INVALID_VALUES)

let cleanNetwork (network : string) = network.Trim('.') + "."

let printNetworkIpsStatus activeOnly separator ipsInfo =
    let filterFun = Array.filter snd
    let separator = Regex.Unescape(separator)

    ipsInfo
    |> (if activeOnly then filterFun else id)
    |> Array.iter (fun (ip, status) -> Console.WriteLine $"%s{ip}%s{separator}%b{status}")
