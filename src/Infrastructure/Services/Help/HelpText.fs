namespace Services.Help.HelpText

open System
open System.Text
open CommandLine
open Model
open Model.Constants

open type Infrastructure.DI.Brokers.HelpDI.IIHelpTextBroker

type Service () =

    //----------------------------------------------------------------------------------------------------
    static let buildOptionAttributeLine (option : OptionAttribute) =

        let sbLeft = StringBuilder()

        sbLeft
            .Append("".PadLeft LEFT_MARGIN)
            .Append($"-{option.ShortName}")
        |> ignore

        if option.LongName |> (not << String.IsNullOrWhiteSpace) then
            sbLeft.Append($",  --{option.LongName}") |> ignore


        let sbRight = StringBuilder()
        sbRight.Append option.HelpText |> ignore

        if option.Default <> null then
            sbRight.Append($" (def: {option.Default})") |> ignore

        ArgLineInfo (sbLeft.ToString(), sbRight.ToString())
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let buildValueAttributeLine (valAttr : ValueAttribute) =

        let sbLeft = StringBuilder()

        sbLeft
            .Append("".PadLeft LEFT_MARGIN)
            .Append(valAttr.MetaName)
        |> ignore

        if valAttr.Required then
            sbLeft.Append(" (requerido)") |> ignore


        let sbRight = StringBuilder()
        sbRight.Append valAttr.HelpText |> ignore

        if valAttr.Default <> null then
            sbRight.Append($" (def: {valAttr.Default})") |> ignore

        ArgLineInfo (sbLeft.ToString(), sbRight.ToString())
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let getArgLinesInfo () =

        let properties = typeof<ArgumentOptions>.GetProperties()

        [|
            for property in properties do
                let customAttributes = property.GetCustomAttributes(true)

                match customAttributes[0] with
                | :? OptionAttribute as opt -> buildOptionAttributeLine opt
                | :? ValueAttribute as value -> buildValueAttributeLine value
                | :? VerbAttribute as verb -> ArgLineInfo (verb.Name, verb.HelpText)
                | _ -> failwith "Atributo no identificado."

            ArgLineInfo ($"""{"".PadLeft LEFT_MARGIN}     --help""", "Muestra esta ayuda y sale.")
            ArgLineInfo ($"""{"".PadLeft LEFT_MARGIN}     --version""",
                         "Devuelve información de la versión y sale.")
        |]
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member showHelp (errors : AppErrors) =

        //------------------------------------------------------------------------------------------------
        let printHelpText (errorList : seq<string>) =

            printHeader ()

            match Seq.head errorList with
            | "HELP" -> printUsage () ; getArgLinesInfo () |> printArgsHelp
            | "VERSION" -> ()
            | _ -> printUsage () ; printErrorList errorList ; getArgLinesInfo () |> printArgsHelp
        //------------------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------------------
        let processArgErrors (errors : seq<Error>) =

            errors
            |> Seq.map (fun error ->
                match error with
                | :? HelpRequestedError -> "HELP"
                | :? VersionRequestedError -> "VERSION"
                | :? MissingRequiredOptionError -> "Falta una opción requerida."
                | :? UnknownOptionError as e -> $"Opción desconocida: {e.Token}"
                | :? BadFormatConversionError as e -> $"{e.NameInfo.LongName}: Error de conversión de valores."
                | _ -> $"Error desconocido. %A{error}")
            |> printHelpText

            match Seq.head errors with
            | :? HelpRequestedError | :? VersionRequestedError -> EXIT_CODE_OK
            | _ -> EXIT_CODE_ARG_ERROR
        //------------------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------------------
        let processInternalErrors (exceptions : seq<Exception>) =

            exceptions
            |> Seq.map (fun e -> e.Message)
            |> printHelpText

            EXIT_CODE_EXCEPTION
        //------------------------------------------------------------------------------------------------

        match errors with
        | ArgErrors argErrors -> processArgErrors argErrors
        | ExceptionErrors exceptions -> processInternalErrors exceptions
    //----------------------------------------------------------------------------------------------------
