namespace Model

open System
open Motsoft.Util
open Model.FileNameValidation

type FileName =
    private FileName of string
        static member private canonicalize (value : string) =
            value
            |> trim

        static member private validateTry (value : string) =

            try
                getValidatorsList ()
                |> Array.iter (fun f -> f value)

                value
            with e -> raise <| AggregateException e

        member this.value = let (FileName value) = this in value
        member this.hasValue = let (FileName value) = this in value |> (not << String.IsNullOrWhiteSpace)

        override this.ToString () = this.value

        static member create (value : string) =
            value
            |> FileName.canonicalize
            |> FileName.validateTry
            |> FileName
