module Model.ValidationHelper

open System
open Motsoft.Util

let checkEmpty (value : string) =
    (value |> String.IsNullOrWhiteSpace || value.Trim '.' |> String.IsNullOrWhiteSpace)
    |> failWithIfTrue "El valor está vacío o sólo tiene puntos."

let checkOctectAreIntsInRange (value : string) =
    value
    |> split "."
    |> Array.iter
           (fun v -> match Int32.TryParse v with
                     | false, _ -> failwith "Los valores de los octetos no son numéricos"
                     | true, intVal when intVal < 0 || intVal > 254 -> failwith "Octetos fuera de rango."
                     | _ -> ())

let checkOctectCount octet (value : string) =
    (value |> split "." |> Array.length <> octet) |> failWithIfTrue "Número equivocado de octetos."
