namespace Services.Help.HelpText

open CommandLine
open Model
open Model.Constants

open type Infrastructure.DI.Brokers.HelpDI.IIHelpTextBroker

type Service () =

    //----------------------------------------------------------------------------------------------------
    static let getArgLinesInfo () =

        let leftSpaces = "".PadLeft 5

        [|
            ArgLineInfo ("-w,  --timeout", "Tiempo en ms de espera en cada ping. (def: 500)")
            ArgLineInfo ("-r,  --retries", "Número pings hasta dar la IP por inactiva. (def: 3)")
            ArgLineInfo ("-s,  --separador", "Separador entre campos. (def: \\t)")
            ArgLineInfo ("-a,  --activos", "Sólo devuelve las IPs activas. (def: False)")
            ArgLineInfo ("-m,  --mac", "Muestra la MAC de cada IP activa. (def: False)")
            ArgLineInfo ("red (requerido)", "La red a escanear.")

            ArgLineInfo ($"{leftSpaces}--help", "Muestra esta ayuda y sale.")
            ArgLineInfo ($"{leftSpaces}--version", "Devuelve información de la versión y sale.")
        |]
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member showHelp (appErrors : AppErrors) =

        //------------------------------------------------------------------------------------------------
        let showHelpText (errorMessages : seq<string>) =

            printHeader ()

            match Seq.head errorMessages with
            | "HELP" -> printUsage () ; getArgLinesInfo () |> printArgsInfo
            | "VERSION" -> ()
            | _ -> printUsage () ; printErrorList errorMessages ; getArgLinesInfo () |> printArgsInfo
        //------------------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------------------
        let processArgErrors (argErrors : ArgErrors) =

            argErrors
            |> Seq.map (fun error ->
                match error with
                | :? HelpRequestedError -> "HELP"
                | :? VersionRequestedError -> "VERSION"
                | :? MissingRequiredOptionError -> "Falta una opción requerida."
                | :? UnknownOptionError as e -> $"Opción desconocida: {e.Token}"
                | :? BadFormatConversionError as e -> $"{e.NameInfo.LongName}: Error de conversión de valores."
                | _ -> $"Error desconocido. %A{error}")
            |> showHelpText

            match Seq.head argErrors with
            | :? HelpRequestedError | :? VersionRequestedError -> EXIT_CODE_OK
            | _ -> EXIT_CODE_ARG_ERROR
        //------------------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------------------
        let processExceptionErrors (exceptionErrors : ExceptionErrors) =

            exceptionErrors
            |> Seq.map (fun e -> e.Message)
            |> showHelpText

            EXIT_CODE_EXCEPTION
        //------------------------------------------------------------------------------------------------

        match appErrors with
        | ArgErrors argErrors -> processArgErrors argErrors
        | ExceptionErrors exceptionErrors -> processExceptionErrors exceptionErrors
    //----------------------------------------------------------------------------------------------------
