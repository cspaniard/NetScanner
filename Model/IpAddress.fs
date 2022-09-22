namespace NetScanner.Model

open Motsoft.Util
open NetScanner.Model.IpAddressValidation

type IpAddress =
    private IpAddress of string
        static member private canonicalize (value : string) =
            value
            |> trim
            |> fun s -> s.Split(".")
            |> join "."

        static member private validate (value : string) =

            getValidatorsList ()
            |> Array.iter (fun f -> f value)

            value

        member this.value = let (IpAddress value) = this in value

        override this.ToString() = this.value

        static member create (value : string) =
            value
            |> IpAddress.canonicalize
            |> IpAddress.validate
            |> IpAddress
