namespace Brokers

open System
open System.IO
open Model
open Motsoft.Util
open DI.Interfaces

type IpBlacklistBroker (fileName : FileName) =

    interface IIpBlacklistBroker with
        //--------------------------------------------------------------------------------------------------------------
        member _.getIpBlacklistTry () =

            match fileName.hasValue with
            | true -> File.ReadAllLines fileName.value
                      |> Array.map (fun l -> l |> split "\t")
                      |> Array.collect id
                      |> Array.filter (not << String.IsNullOrEmpty)
            | false -> Array.empty<string>
        //--------------------------------------------------------------------------------------------------------------
