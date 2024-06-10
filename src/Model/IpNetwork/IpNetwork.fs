namespace Model

open System
open Motsoft.Util
open Model.IpNetworkValidation

type IpNetwork =
    private IpNetwork of string
        //--------------------------------------------------------------------------------------------------------------
        static member private canonicalize (value : string) =
            value
            |> trim
            |> splitWithOptions "." StringSplitOptions.None
            |> join "."
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        static member private validateTry (value : string) =

            getValidatorsList ()
            |> Seq.iterOrAggregateExceptionTry (fun f -> f value)

            value
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        member this.value = let (IpNetwork value) = this in value
        override this.ToString () = this.value
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        static member create (value : string) =
            value
            |> IpNetwork.canonicalize
            |> IpNetwork.validateTry
            |> sprintf "%s."
            |> IpNetwork
        //--------------------------------------------------------------------------------------------------------------
