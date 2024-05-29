namespace Brokers

open System
open System.IO
open DI.Interfaces
open Model
open Motsoft.Util

//----------------------------------------------------------------------------------------------------------------------
[<AbstractClass>]
type BlacklistBrokerBase (fileName : FileName) =

    member _.getBlacklistAsyncTry () =

        backgroundTask {
            match fileName.hasValue with
            | true ->
                let! lines = File.ReadAllLinesAsync fileName.value
                return
                    lines
                    |> Array.map (fun l -> l |> split "\t")
                    |> Array.collect id
                    |> Array.filter (not << String.IsNullOrEmpty)
            | false ->
                return Array.empty<string>
        }
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
type IpBlacklistBroker (fileName : FileName) =
    inherit BlacklistBrokerBase (fileName)

    interface IIpBlacklistBroker with

        member _.getIpBlacklistAsyncTry () =
            base.getBlacklistAsyncTry ()
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
type MacBlacklistBroker (fileName : FileName) =
    inherit BlacklistBrokerBase (fileName)

    interface IMacBlacklistBroker with

        member _.getMacBlacklistAsyncTry () =
            base.getBlacklistAsyncTry ()
//----------------------------------------------------------------------------------------------------------------------
