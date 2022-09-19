module NetScanner.HelpText

open CommandLine
open NetScanner.Options
open NetScanner.HelpTextHelper

let showHelpText (err : NotParsed<ArgumentOptions>) =

    //----------------------------------------------------------------------------------------------------
    let printHelpText (errorList : seq<string>) =

        printHeader ()

        match Seq.head errorList with
        | "HELP" -> printUsage () ; printArgsHelp err
        | "VERSION" -> ()
        | _ -> printUsage () ; printErrorList errorList ; printArgsHelp err
    //----------------------------------------------------------------------------------------------------

    err.Errors
    |> Seq.map (fun error ->
        match error with
        | :? HelpRequestedError -> "HELP"
        | :? VersionRequestedError -> "VERSION"
        | :? MissingRequiredOptionError -> "Falta una opción requerida."
        | :? UnknownOptionError as e -> $"Opción desconocida: {e.Token}"
        | :? BadFormatConversionError as e -> $"Error de conversión de valores: {e.NameInfo.NameText}"
        | _ -> $"Error desconocido. %A{error}")
    |> printHelpText
