module NetScanner.ArgumentOptionsValidation

open System.ComponentModel.DataAnnotations
open CommandLine
open DI.Interfaces
open Model
open Model.Definitions

//----------------------------------------------------------------------------------------------------------------------
let getOptionValidationErrors (options : ArgumentOptions) =

    let getValidationException f v =
        try
            f v |> ignore
            None
        with
        | :? ValidationException as e -> Some e
        | _ -> failwith "Alguna de Las validaciones no devuelve un error de tipo ValidationException."

    seq {
        getValidationException PingTimeOut.create options.PingTimeOut
        getValidationException Retries.create options.Retries
        getValidationException NameLookupTimeOut.create options.NameLookUpTimeOut
        getValidationException FileName.create options.MacBlackListFileName
        getValidationException FileName.create options.IpBlackListFileName
        getValidationException IpNetwork.create options.Network
    }
    |> Seq.choose id
    |> ValidationErrors
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
let ifOptionErrorsShowAndExit (helpTextService : IHelpTextService) (parserResult : ParserResult<ArgumentOptions>) =

    //------------------------------------------------------------------------------------------------------------------
    let showErrorsAndExit (errors: AppErrors) =
            errors
            |> helpTextService.showHelp
            |> exit
    //------------------------------------------------------------------------------------------------------------------

    match parserResult with
    | Parsed as parsed ->

        match getOptionValidationErrors parsed.Value with
        | ValidationErrors ve when ve |> Seq.isEmpty ->
            ()
        | validationErrors ->
            showErrorsAndExit validationErrors

    | NotParsed as notParsed ->
            showErrorsAndExit (notParsed.Errors |> ArgErrors)
//----------------------------------------------------------------------------------------------------------------------
