namespace Brokers

open System
open System.Reflection
open System.Runtime.InteropServices
open Model
open Model.Constants
open DI.Interfaces

type HelpTextBroker () =

    interface IHelpTextBroker with
        //--------------------------------------------------------------------------------------------------------------
        member _.printHeaderTry () =

            if RuntimeInformation.IsOSPlatform OSPlatform.Windows then Console.WriteLine ()

            let version = Assembly.GetEntryAssembly().GetName().Version

            Console.WriteLine $"netscanner - {version.Major}.{version.Minor}.{version.Build}"
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        member _.printUsageTry () =

            Console.WriteLine "\nUSO: netscanner [opciones] red"
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        member _.printErrorSectionTry errorList =

            if errorList |> (not << Seq.isEmpty) then
                let leftMargin = String (' ', LEFT_MARGIN)

                Console.WriteLine "\nERRORES:"

                if RuntimeInformation.IsOSPlatform OSPlatform.Windows then Console.WriteLine ()

                errorList
                |> Seq.filter (not << String.IsNullOrWhiteSpace)
                |> Seq.iter (fun errorLine -> Console.Error.WriteLine $"{leftMargin}{errorLine}")
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        member _.printArgsInfoTry argLineInfoList =

            let maxWidth = argLineInfoList |> Array.map (fun (ArgLineInfo (n, _)) -> n.Length) |> Array.max
            let leftMargin = String (' ', LEFT_MARGIN)

            Console.WriteLine "\nOPCIONES:\n"

            argLineInfoList
            |> Array.iter (fun (ArgLineInfo (names, helpText)) ->
                               Console.WriteLine $"{leftMargin}{names.PadRight maxWidth}    {helpText}\n")
        //--------------------------------------------------------------------------------------------------------------
