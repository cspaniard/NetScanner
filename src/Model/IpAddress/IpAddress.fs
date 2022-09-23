namespace Model

open Motsoft.Util
open Model.IpAddressValidation

type IpAddress =
    private IpAddress of string
        static member private canonicalize (value : string) =
            value
            |> trim
            |> fun s -> s.Split(".")
            |> join "."

        static member private validateTry (value : string) =

            try
                getValidatorsList ()
                |> Array.iter (fun f -> f value)

                value
            with e -> failwith $"IpAddress: {e.Message}"

        member this.value = let (IpAddress value) = this in value

        override this.ToString() = this.value

        static member create (value : string) =
            value
            |> IpAddress.canonicalize
            |> IpAddress.validateTry
            |> IpAddress
