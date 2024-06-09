module Model.MacValidation

open Motsoft.Util

let checkNull (macString : string) (orginalValue : string) =
    macString = null
    |> failWithIfTrue $"Mac ({orginalValue}): El dato proporcionado no puede ser null."

let checkEvenLength (macString : string) (orginalValue : string) =
    macString.Length % 2 = 0
    |> failWithIfFalse $"Mac ({orginalValue}): La longitud tiene que ser mútiplo de 2."

let checkValidChars (macString : string) (orginalValue : string) =
    let validChars = Array.append [| '0'..'9' |] [| 'A'..'F' |]

    macString.ToCharArray ()
    |> Array.iter (
        fun c ->
            validChars |>
            Array.contains c
            |> failWithIfFalse $"Mac ({orginalValue}): Contiene caracteres inválidos.")


let getValidatorsList () =
    [|
        checkNull
        checkEvenLength
        checkValidChars
    |]
