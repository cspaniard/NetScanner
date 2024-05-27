namespace Brokers

open System
open System.IO
open DI.Interfaces
open Model
open Motsoft.Util

type MacBlacklistBroker (fileName : FileName) =

    interface IMacBlacklistBroker with
        //--------------------------------------------------------------------------------------------------------------
        member _.getMacBlacklistTry () =

            match fileName.hasValue with
            | true -> File.ReadAllLines fileName.value
                      |> Array.map (fun l -> l |> split "\t")
                      |> Array.collect id
                      |> Array.filter (not << String.IsNullOrEmpty)
            | false -> Array.empty<string>
        //--------------------------------------------------------------------------------------------------------------
