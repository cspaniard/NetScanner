namespace Services

open System
open System.ComponentModel.DataAnnotations
open CommandLine
open Model
open Model.Constants
open DI.Interfaces

type HelpTextService (HelpTextBroker : IHelpTextBroker) =

    //------------------------------------------------------------------------------------------------------------------
    let getArgLinesInfo () =

        let leftSpaces = String (' ', 5)

        [|
            ArgLineInfo ("-w,  --ping-timeout", $"Tiempo de espera en ms para cada ping. (def: {DEF_PING_TIMEOUT})")
            ArgLineInfo ("-r,  --reintentos", $"Número de pings hasta dar la IP por inactiva. (def: {DEF_RETRIES})")
            ArgLineInfo ("-s,  --separador", "Separador entre campos. (def: \\t)")
            ArgLineInfo ("-a,  --activos", "Sólo devuelve las IPs activas. (def: False)")
            ArgLineInfo ("-m,  --mac", "Muestra la MAC de cada IP activa. (def: False)")
            ArgLineInfo ("-n,  --nombres", "Muestra los nombres de los dispositivos (def: False)")
            ArgLineInfo ("-l,  --nombres-timeout",
                         $"Tiempo de espera en ms para cada resolución de nombre. (def: {DEF_NAME_LOOKUP_TIMEOUT})")
            ArgLineInfo ("-t,  --ttl", "Muestra el TTL de cada IP activa. (def: False)")
            ArgLineInfo ("-D,  --dns", "Resuelve los nombres usando DNS. (def: False)")
            ArgLineInfo ("-d,  --debug", "Muestra estadísticas de tiempo. (def: False)")
            ArgLineInfo ("-b,  --blacklist", "Fichero con las MACs de los dispositivos a ignorar. (def: \"\")")
            ArgLineInfo ("-B   --ip-blacklist", "Fichero con las IPs de los dispositivos a ignorar. (def: \"\")")
            ArgLineInfo ("red (requerido)", "La red a escanear.")

            ArgLineInfo ($"{leftSpaces}--help", "Muestra esta ayuda y sale.")
            ArgLineInfo ($"{leftSpaces}--version", "Devuelve información de la versión y sale.")
        |]
    //------------------------------------------------------------------------------------------------------------------

    interface IHelpTextService with
        //--------------------------------------------------------------------------------------------------------------
        member _.showHelpTry (appErrors : AppErrors) =

            //----------------------------------------------------------------------------------------------------------
            let showHelpText (errorMessages : seq<string>) =

                //------------------------------------------------------------------------------------------------------
                let showHelpText (errorMessages : seq<string>) =

                    HelpTextBroker.printHeaderTry ()
                    HelpTextBroker.printUsageTry ()
                    HelpTextBroker.printErrorSectionTry errorMessages
                    HelpTextBroker.printArgsInfoTry <| getArgLinesInfo ()
                //------------------------------------------------------------------------------------------------------

                match Seq.head errorMessages with
                | "VERSION" -> HelpTextBroker.printHeaderTry ()
                | "HELP" -> showHelpText Seq.empty
                | _ -> showHelpText errorMessages
            //----------------------------------------------------------------------------------------------------------

            //----------------------------------------------------------------------------------------------------------
            let processArgErrors (argErrors : ArgErrors) =

                argErrors
                |> Seq.map (fun error ->
                    match error with
                    | :? HelpRequestedError -> "HELP"
                    | :? VersionRequestedError -> "VERSION"
                    | :? MissingRequiredOptionError -> "red: Este valor es requerido."
                    | :? UnknownOptionError as e -> $"Opción desconocida: {e.Token}"
                    | :? BadFormatConversionError as e -> $"{e.NameInfo.LongName}: Error de conversión de valores."
                    | _ -> $"Error desconocido. %A{error}")
                |> showHelpText

                match Seq.head argErrors with
                | :? HelpRequestedError | :? VersionRequestedError -> EXIT_CODE_OK
                | _ -> EXIT_CODE_ARG_ERROR
            //----------------------------------------------------------------------------------------------------------

            //----------------------------------------------------------------------------------------------------------
            let processExceptionErrors (exceptionErrors : ExceptionErrors) =

                exceptionErrors
                |> Seq.map _.Message
                |> showHelpText

                EXIT_CODE_EXCEPTION
            //----------------------------------------------------------------------------------------------------------

            //----------------------------------------------------------------------------------------------------------
            let processValidationErrors (validationErrors : ValidationException seq) =

                validationErrors
                |> Seq.map _.Message
                |> showHelpText

                EXIT_CODE_ARG_ERROR
            //----------------------------------------------------------------------------------------------------------

            match appErrors with
            | ArgErrors argErrors -> processArgErrors argErrors
            | ExceptionErrors exceptionErrors -> processExceptionErrors exceptionErrors
            | ValidationError validationError -> processValidationErrors [ validationError ]
            | ValidationErrors validationErrors -> processValidationErrors validationErrors
        //--------------------------------------------------------------------------------------------------------------
