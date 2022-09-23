namespace Brokers.Help.HelpText

open System
open System.Reflection
open Model
open Model.Constants

type Broker () =

    //----------------------------------------------------------------------------------------------------
    static member printHeader () =

        let assemblyName = Assembly.GetEntryAssembly().GetName()
        let version = assemblyName.Version

        Console.WriteLine($"{assemblyName.Name} - {version.Major}.{version.Minor}.{version.Build}")
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member printUsage () =
        let assemblyName = Assembly.GetEntryAssembly().GetName()

        Console.WriteLine $"\nUSO: {assemblyName.Name} [opciones] red"
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member printArgsHelp (argLinesInfo : ArgLineInfo[]) =

        let maxWidth = argLinesInfo |> Array.map (fun (ArgLineInfo (n, _)) -> n.Length) |> Array.max
        Console.WriteLine ()

        argLinesInfo
        |> Array.iter (fun (ArgLineInfo (n, h)) -> Console.WriteLine $"{n.PadRight maxWidth}    {h}\n")
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member printErrorList errorList =

        Console.WriteLine "\nERRORES:"

        errorList
        |> Seq.filter (not << String.IsNullOrWhiteSpace)
        |> Seq.iter (fun errorLine -> Console.WriteLine $"""{("".PadLeft LEFT_MARGIN)}{errorLine}""")
    //----------------------------------------------------------------------------------------------------
