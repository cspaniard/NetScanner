namespace Brokers.Help.HelpText

open System
open System.Reflection
open System.Runtime.InteropServices
open Model
open Model.Constants

type Broker () =

    //------------------------------------------------------------------------------------------------------------------
    static member printHeader () =

        if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
            Console.WriteLine()

        let version = Assembly.GetEntryAssembly().GetName().Version

        Console.WriteLine($"netscanner - {version.Major}.{version.Minor}.{version.Build}")
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member printUsage () =

        Console.WriteLine "\nUSO: netscanner [opciones] red"
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member printErrorSection errorList =

        if errorList |> (not << Seq.isEmpty) then
            let leftMargin = String(' ', LEFT_MARGIN)

            Console.WriteLine "\nERRORES:"

            if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then Console.WriteLine ""

            errorList
            |> Seq.filter (not << String.IsNullOrWhiteSpace)
            |> Seq.iter (fun errorLine -> Console.WriteLine $"{leftMargin}{errorLine}")
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member printArgsInfo argLineInfoList =

        let maxWidth = argLineInfoList |> Array.map (fun (ArgLineInfo (n, _)) -> n.Length) |> Array.max
        let leftMargin = String(' ', LEFT_MARGIN)

        Console.WriteLine ()

        argLineInfoList
        |> Array.iter (fun (ArgLineInfo (names, helpText)) ->
                           Console.WriteLine $"{leftMargin}{names.PadRight maxWidth}    {helpText}\n")
    //------------------------------------------------------------------------------------------------------------------
