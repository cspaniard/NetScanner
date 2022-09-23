namespace Services.Help.HelpText

open System
open System.Text
open CommandLine
open Model.Options
open Model.HelpTextHelper

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

        ArgLinesInfo (sbLeft.ToString(), sbRight.ToString())
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

        ArgLinesInfo (sbLeft.ToString(), sbRight.ToString())
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let getArgLinesInfo (err : NotParsed<ArgumentOptions>) =

        let properties = err.TypeInfo.Current.GetProperties()

        [|
            for property in properties do
                let customAttributes = property.GetCustomAttributes(true)

                match customAttributes[0] with
                | :? OptionAttribute as opt -> buildOptionAttributeLine opt
                | :? ValueAttribute as value -> buildValueAttributeLine value
                | :? VerbAttribute as verb -> ArgLinesInfo (verb.Name, verb.HelpText)
                | _ -> failwith "Atributo no identificado."
        |]
    //----------------------------------------------------------------------------------------------------

    static member showHelpText (errors : NotParsed<ArgumentOptions>) =

        //----------------------------------------------------------------------------------------------------
        let printHelpText (errorList : seq<string>) =

            printHeader ()

            match Seq.head errorList with
            | "HELP" -> printUsage () ; getArgLinesInfo errors |> printArgsHelp
            | "VERSION" -> ()
            | _ -> printUsage () ; printErrorList errorList ; getArgLinesInfo errors |> printArgsHelp
        //----------------------------------------------------------------------------------------------------

        errors.Errors
        |> Seq.map (fun error ->
            match error with
            | :? HelpRequestedError -> "HELP"
            | :? VersionRequestedError -> "VERSION"
            | :? MissingRequiredOptionError -> "Falta una opción requerida."
            | :? UnknownOptionError as e -> $"Opción desconocida: {e.Token}"
            | :? BadFormatConversionError as e -> $"Error de conversión de valores: {e.NameInfo.NameText}"
            | _ -> $"Error desconocido. %A{error}")
        |> printHelpText
