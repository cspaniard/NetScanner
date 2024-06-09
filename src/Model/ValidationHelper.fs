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

let checkEmptyTry (errors : ErrorDict) (value : string) =
    value
    |> String.IsNullOrWhiteSpace
    |> failWithIfTrue $"IpAddress ({value}): {errors[ValueIsEmpty]}"

let checkOctectsForSpacesOrEmptyTry (errors : ErrorDict) (value : string) =
    value
    |> split "."
    |> Array.iter (fun o -> o.Contains " "
                            |> failWithIfTrue $"IpAddress ({value}): {errors[ValueContainsSpaces]}"

                            o
                            |> String.IsNullOrWhiteSpace
                            |> failWithIfTrue $"IpAddress ({value}): {errors[OctectIsEmpty]}")

let checkOctectsAreIntsInRangeTry (errors : ErrorDict) (value : string) =
    value
    |> split "."
    |> Array.iter
           (fun o -> match Int32.TryParse o with
                     | false, _ ->
                         failwith $"IpAddress ({value}): {errors[OctectIsNotNumber]}"
                     | true, intVal when intVal < 0 || intVal > 254 ->
                         failwith $"IpAddress ({value}): {errors[OctectOutOfRange]}"
                     | _ -> ())

let checkOctectCountTry (errors : ErrorDict) octetCount (value : string) =
    value
    |> split "."
    |> Array.length <> octetCount
    |> failWithIfTrue $"IpAddress ({value}): {errors[OctectIncorrectCount]}"
