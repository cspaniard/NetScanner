module Model.FileNameValidation

open System
open System.IO
open Motsoft.Util

let checkNull (fileName : string) =
    fileName = null |> failWithIfTrue "El dato proporcionado no puede ser null."

let checkFileExists (fileName : string) =
    if fileName |> (not << String.IsNullOrWhiteSpace) then
        fileName |> File.Exists |> failWithIfFalse $"El fichero {fileName} no existe."


let getValidatorsList () =
    [|
       checkNull
       checkFileExists
    |]
