namespace Brokers.Storage.Blacklist

open System.IO
open Model
open Motsoft.Util

type Broker () =

    static let mutable fileName = FileName.create ""

    static member FileName
        with get() = fileName

    static member init (options : ArgumentOptions) =
        fileName <- options.BlackListFile |> FileName.create

    static member getMacBlacklistTry () =

        match Broker.FileName.hasValue with
        | true -> File.ReadAllText(Broker.FileName.value) |> split "\t"
        | false -> Array.empty<string>
