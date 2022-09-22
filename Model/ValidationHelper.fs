module Model.ValidationHelper

open System
open Motsoft.Util

let checkEmpty (value : string) =
    value |> String.IsNullOrWhiteSpace |> failWithIfTrue "El valor está vacío."

let checkOctectForSpacesOrEmpty (value : string) =
    value
    |> fun s -> s.Split(".")
    |> Array.iter (fun o -> o.Contains " " |> failWithIfTrue "El valor contiene espacios."
                            o |> String.IsNullOrWhiteSpace |> failWithIfTrue "Octetos vacíos.")

let checkOctectAreIntsInRange (value : string) =
    value
    |> split "."
    |> Array.iter
           (fun o -> match Int32.TryParse o with
                     | false, _ -> failwith "Los valores de los octetos no son numéricos"
                     | true, intVal when intVal < 0 || intVal > 254 -> failwith "Octetos fuera de rango."
                     | _ -> ())

let checkOctectCount octet (value : string) =
    (value |> split "." |> Array.length <> octet) |> failWithIfTrue "Número incorrecto de octetos."
