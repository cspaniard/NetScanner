module Model.RetriesValidation

open Motsoft.Util

let checkPostiveTry value =
    value >= 0 |> failWithIfFalse "El valor tiene que ser positivo."


let getValidatorsList () =
    [|
        checkPostiveTry
    |]
