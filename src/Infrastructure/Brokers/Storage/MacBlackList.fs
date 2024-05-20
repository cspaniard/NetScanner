namespace Brokers.Storage.MacBlacklist

open System
open System.IO
open Model
open Motsoft.Util

type Broker () =

    static let mutable _fileName = FileName.create ""

    //------------------------------------------------------------------------------------------------------------------
    static member init fileName =
        _fileName <- fileName
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member FileName with get () = _fileName
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member getMacBlacklistTry () =

        // TODO: change to checkInit() pattern.

        match Broker.FileName.hasValue with
        | true -> File.ReadAllLines Broker.FileName.value
                  |> Array.map (fun l -> l |> split "\t")
                  |> Array.collect id
                  |> Array.filter (not << String.IsNullOrEmpty)
        | false -> Array.empty<string>
    //------------------------------------------------------------------------------------------------------------------
