namespace NetScanner.Model

open Motsoft.Util
open NetScanner.Model.IpNetworkValidation

type IpNetwork =
    private IpNetwork of string
        static member private validate (value : string) =

            getValidatorsList ()
            |> Array.iter (fun f -> f value)

            value

        static member private canonicalize (value : string) =
            value
            |> split "."
            |> Array.map trim
            |> join "."
            |> sprintf "%s."

        member this.value = let (IpNetwork value) = this in value
        override this.ToString() = this.value

        static member create (value : string) =
            value
            |> IpNetwork.canonicalize
            |> IpNetwork.validate
            |> IpNetwork
