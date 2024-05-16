namespace Model

open System
open Motsoft.Util
open Model.IpAddressValidation

[<CustomComparison ; CustomEquality>]
type IpAddress =
    private IpAddress of string
        static member private canonicalize (value : string) =
            value
            |> trim
            |> splitWitchOptionsByStringChars "." StringSplitOptions.None
            |> join "."

        static member private validateTry (value : string) =

            try
                getValidatorsList ()
                |> Array.iter (fun f -> f value)

                value
            with e -> failwith $"IpAddress: {e.Message}"

        member this.value = let (IpAddress value) = this in value

        static member create (value : string) =
            value
            |> IpAddress.canonicalize
            |> IpAddress.validateTry
            |> IpAddress

        override this.ToString () = this.value

        override this.Equals(obj) =
            match obj with
            | :? IpAddress as other -> this.value = other.value
            | _ -> false

        override this.GetHashCode() =
            this.value.GetHashCode()

    interface IComparable with
        member this.CompareTo(otherIp : obj) =
            let otherIp = otherIp :?> IpAddress
            let thisIpCompValue = this.value |> split "." |> Array.last |> int
            let otherIpCompValue = otherIp.value |> split "." |> Array.last |> int
            compare thisIpCompValue otherIpCompValue
