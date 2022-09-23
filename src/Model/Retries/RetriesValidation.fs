module Model.RetriesValidation

open Motsoft.Util

let checkPostive value =
    value >= 0 |> failWithIfFalse "El valor tiene que ser positivo."


let getValidatorsList () =
    [|
        checkPostive
    |]
