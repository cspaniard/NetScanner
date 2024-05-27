namespace Brokers

open System
open System.IO
open DI.Interfaces
open Model
open Motsoft.Util

//----------------------------------------------------------------------------------------------------------------------
[<AbstractClass>]
type BlacklistBrokerBase (fileName : FileName) =

    member _.getBlacklistTry () =

        match fileName.hasValue with
        | true -> File.ReadAllLines fileName.value
                  |> Array.map (fun l -> l |> split "\t")
                  |> Array.collect id
                  |> Array.filter (not << String.IsNullOrEmpty)
        | false -> Array.empty<string>
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
type IpBlacklistBroker (fileName : FileName) =
    inherit BlacklistBrokerBase (fileName)

    interface IIpBlacklistBroker with

        member _.getIpBlacklistTry () =
            base.getBlacklistTry ()
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
type MacBlacklistBroker (fileName : FileName) =
    inherit BlacklistBrokerBase (fileName)

    interface IMacBlacklistBroker with

        member _.getMacBlacklistTry () =
            base.getBlacklistTry ()
//----------------------------------------------------------------------------------------------------------------------
