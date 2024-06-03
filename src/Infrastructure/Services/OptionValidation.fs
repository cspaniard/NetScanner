namespace Services

open System.ComponentModel.DataAnnotations
open CommandLine
open DI.Interfaces
open Model
open Model.Definitions

type OptionValidationService (HelpTextService : IHelpTextService) =

    //------------------------------------------------------------------------------------------------------------------
    let getOptionValidationErrors (options : ArgumentOptions) =

        let getValidationExceptionOrNone f v =
            try
                f v |> ignore
                None
            with
            | :? ValidationException as e -> Some e
            | _ -> failwith "Alguna de Las validaciones no devuelve un error de tipo ValidationException."

        seq {
            getValidationExceptionOrNone PingTimeOut.create options.PingTimeOut
            getValidationExceptionOrNone Retries.create options.Retries
            getValidationExceptionOrNone NameLookupTimeOut.create options.NameLookUpTimeOut
            getValidationExceptionOrNone FileName.create options.MacBlackListFileName
            getValidationExceptionOrNone FileName.create options.IpBlackListFileName
            getValidationExceptionOrNone IpNetwork.create options.Network
        }
        |> Seq.choose id
        |> ValidationErrors
    //------------------------------------------------------------------------------------------------------------------

    interface IOptionValidationService with
        //--------------------------------------------------------------------------------------------------------------
        member _.ifErrorsShowAndExit (parserResult : ParserResult<ArgumentOptions>) =

            //----------------------------------------------------------------------------------------------------------
            let showErrorsAndExit (errors: AppErrors) =
                    errors
                    |> HelpTextService.showHelp
                    |> exit
            //----------------------------------------------------------------------------------------------------------

            match parserResult with
            | Parsed as parsed ->

                match getOptionValidationErrors parsed.Value with
                | ValidationErrors ve when ve |> Seq.isEmpty ->
                    ()
                | validationErrors ->
                    showErrorsAndExit validationErrors

            | NotParsed as notParsed ->
                    showErrorsAndExit (notParsed.Errors |> ArgErrors)
        //--------------------------------------------------------------------------------------------------------------
