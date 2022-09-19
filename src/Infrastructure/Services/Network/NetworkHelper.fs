module Services.Network.NetworkHelper

open System
open Motsoft.Util

//----------------------------------------------------------------------------------------------------
let [<Literal>] BAD_OCTET_COUNT = "Número incorrecto de octetos."
let [<Literal>] NETWORK_INVALID_CHARS = "La red sólo puede contener números."
let [<Literal>] NETWORK_INVALID_VALUES = "Valores inválidos en la red."
//----------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------
let validateNetworkTry network =
    let networkParts = network |> split "."

    networkParts.Length = 3 |> failWithIfFalse BAD_OCTET_COUNT

    networkParts
    |> Array.iter (fun np ->
                       let result, value = Int32.TryParse np
                       result |> failWithIfFalse NETWORK_INVALID_CHARS
                       (value < 0 || value > 254) |> failWithIfTrue NETWORK_INVALID_VALUES)

    network.Trim('.') + "."
//----------------------------------------------------------------------------------------------------
