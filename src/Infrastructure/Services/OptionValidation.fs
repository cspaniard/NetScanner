namespace Services

open System
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
                seq { None }
            with
            | :? AggregateException as e ->
                    seq {
                        for ex in e.Flatten().InnerExceptions do
                            match ex with
                            | :? ValidationException as ve -> yield Some ve
                            | _ ->
                                failwith "Alguna de Las validaciones no devuelve un error de tipo ValidationException."
                    }
            | _ -> failwith "No se ha recibido un AggregateException con los errores de validación."

        seq {
            yield! getValidationExceptionOrNone PingTimeOut.create options.PingTimeOut
            yield! getValidationExceptionOrNone Retries.create options.Retries
            yield! getValidationExceptionOrNone NameLookupTimeOut.create options.NameLookUpTimeOut
            yield! getValidationExceptionOrNone FileName.create options.MacBlackListFileName
            yield! getValidationExceptionOrNone FileName.create options.IpBlackListFileName
            yield! getValidationExceptionOrNone IpNetwork.create options.Network
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
                    |> HelpTextService.showHelpTry
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
