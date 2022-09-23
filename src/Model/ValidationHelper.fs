module Model.ValidationHelper

open System
open System.Collections.Generic
open Motsoft.Util

type Errors =
    | ValueIsEmpty
    | ValueContainsSpaces
    | OctectIsEmpty
    | OctectIsNotNumber
    | OctectOutOfRange
    | OctectIncorrectCount

type ErrorDict = Dictionary<Errors, string>

let checkEmpty (errors : ErrorDict) (value : string) =
    value |> String.IsNullOrWhiteSpace |> failWithIfTrue errors[ValueIsEmpty]

let checkOctectsForSpacesOrEmpty (errors : ErrorDict) (value : string) =
    value
    |> fun s -> s.Split(".")
    |> Array.iter (fun o -> o.Contains " " |> failWithIfTrue errors[ValueContainsSpaces]
                            o |> String.IsNullOrWhiteSpace |> failWithIfTrue errors[OctectIsEmpty])

let checkOctectsAreIntsInRange (errors : ErrorDict) (value : string) =
    value
    |> split "."
    |> Array.iter
           (fun o -> match Int32.TryParse o with
                     | false, _ -> failwith errors[OctectIsNotNumber]
                     | true, intVal when intVal < 0 || intVal > 254 -> failwith errors[OctectOutOfRange]
                     | _ -> ())

let checkOctectCount (errors : ErrorDict) octet (value : string) =
    (value |> split "." |> Array.length <> octet) |> failWithIfTrue errors[OctectIncorrectCount]
