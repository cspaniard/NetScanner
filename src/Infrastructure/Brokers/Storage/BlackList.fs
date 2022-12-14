namespace Brokers.Storage.Blacklist

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

        match Broker.FileName.hasValue with
        | true -> File.ReadAllText Broker.FileName.value |> split "\t"
        | false -> Array.empty<string>
    //------------------------------------------------------------------------------------------------------------------
