module NetScanner.HelpTextHelper

open System
open System.Reflection
open System.Text
open CommandLine
open NetScanner.Options

let [<Literal>] LEFT_MARGIN = 2

// TODO: Posiblemente partir e implementar como Service y Broker.

//----------------------------------------------------------------------------------------------------
let buildOptionAttributeLine (option : OptionAttribute) =

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

    (sbLeft.ToString(), sbRight.ToString())
//----------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------
let buildValueAttributeLine (valAttr : ValueAttribute) =

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

    (sbLeft.ToString(), sbRight.ToString())
//----------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------
let getArgsLinesInfo (err : NotParsed<ArgumentOptions>) =

    let properties = err.TypeInfo.Current.GetProperties()

    [|
        for property in properties do
            let customAttributes = property.GetCustomAttributes(true)

            match customAttributes[0] with
            | :? OptionAttribute as opt -> buildOptionAttributeLine opt
            | :? ValueAttribute as value -> buildValueAttributeLine value
            | :? VerbAttribute as verb -> (verb.Name, verb.HelpText)
            | _ -> failwith "Atributo no identificado."
    |]
//----------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------
let printHeader () =

    let assemblyName = Assembly.GetEntryAssembly().GetName()
    Console.WriteLine($"{assemblyName.Name} - {assemblyName.Version}")
//----------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------
let printUsage () =
    let assemblyName = Assembly.GetEntryAssembly().GetName()

    Console.WriteLine $"\nUSO: {assemblyName.Name} [opciones] red"
//----------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------
let printArgsHelp (err : NotParsed<ArgumentOptions>) =

    let argsLinesInfo = getArgsLinesInfo(err)
    let maxWidth = argsLinesInfo |> Array.map (fun (l, _) -> l.Length) |> Array.max

    Console.WriteLine ()
    argsLinesInfo
    |> Array.iter (fun (l, r) -> Console.WriteLine $"{l.PadRight maxWidth}    {r}\n")
//----------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------
let printErrorList errorList =

    let leftMargin = "".PadLeft LEFT_MARGIN
    Console.WriteLine "\nERRORES:"

    errorList
    |> Seq.filter (not << String.IsNullOrWhiteSpace)
    |> Seq.iter (fun errorLine -> Console.WriteLine $"{leftMargin}{errorLine}")
//----------------------------------------------------------------------------------------------------
