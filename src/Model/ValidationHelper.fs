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
    |> failWithIfTrue (String.Format(errors[ValueIsEmpty], value))

let checkOctectsForSpacesOrEmptyTry (errors : ErrorDict) (value : string) =
    value
    |> split "."
    |> Array.iter (fun o -> o.Contains " "
                            |> failWithIfTrue (String.Format(errors[ValueContainsSpaces], value))

                            o
                            |> String.IsNullOrWhiteSpace
                            |> failWithIfTrue (String.Format(errors[OctectIsEmpty], value)))

let checkOctectsAreIntsInRangeTry (errors : ErrorDict) (value : string) =
    value
    |> split "."
    |> Array.iter
           (fun o -> match Int32.TryParse o with
                     | false, _ ->
                         failwith (String.Format(errors[OctectIsNotNumber], value))
                     | true, intVal when intVal < 0 || intVal > 254 ->
                         failwith (String.Format(errors[OctectOutOfRange], value))
                     | _ -> ())

let checkOctectCountTry (errors : ErrorDict) octetCount (value : string) =
    value
    |> split "."
    |> Array.length <> octetCount
    |> failWithIfTrue (String.Format(errors[OctectIncorrectCount], value))
