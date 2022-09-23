namespace Brokers.Help.HelpText

open System
open System.Reflection
open Model.HelpTextHelper

type Broker () =

    //----------------------------------------------------------------------------------------------------
    static member printHeader () =

        let assemblyName = Assembly.GetEntryAssembly().GetName()
        Console.WriteLine($"{assemblyName.Name} - {assemblyName.Version}")
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member printUsage () =
        let assemblyName = Assembly.GetEntryAssembly().GetName()

        Console.WriteLine $"\nUSO: {assemblyName.Name} [opciones] red"
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member printArgsHelp (argLinesInfo : ArgLinesInfo[]) =

        let maxWidth = argLinesInfo |> Array.map (fun (ArgLinesInfo (l, _)) -> l.Length) |> Array.max
        Console.WriteLine ()

        argLinesInfo
        |> Array.iter (fun (ArgLinesInfo (l, r)) -> Console.WriteLine $"{l.PadRight maxWidth}    {r}\n")
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member printErrorList errorList =

        Console.WriteLine "\nERRORES:"

        errorList
        |> Seq.filter (not << String.IsNullOrWhiteSpace)
        |> Seq.iter (fun errorLine -> Console.WriteLine $"""{("".PadLeft LEFT_MARGIN)}{errorLine}""")
    //----------------------------------------------------------------------------------------------------
