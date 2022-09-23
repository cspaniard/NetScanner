namespace NetScanner.Model

open Motsoft.Util
open NetScanner.Model.IpNetworkValidation

type IpNetwork =
    private IpNetwork of string
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
            with e -> failwith $"red: {e.Message}"

        member this.value = let (IpNetwork value) = this in value
        override this.ToString() = this.value

        static member create (value : string) =
            value
            |> IpNetwork.canonicalize
            |> IpNetwork.validateTry
            |> sprintf "%s."
            |> IpNetwork
