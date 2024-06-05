module Model.MacValidation

open Motsoft.Util

let checkNull (macString : string) =
    macString = null |> failWithIfTrue "El dato proporcionado no puede ser null."

let checkEvenLength (macString : string) =
    macString.Length % 2 = 0 |> failWithIfFalse "La longitud tiene que ser mútiplo de 2."

let checkValidChars (macString : string) =
    let validChars = Array.append [| '0'..'9' |] [| 'A'..'F' |]

    macString.ToCharArray ()
    |> Array.iter (fun c -> validChars |> Array.contains c |> failWithIfFalse "Contiene caracteres inválidos.")


let getValidatorsList () =
    [|
        checkNull
        checkEvenLength
        checkValidChars
    |]
