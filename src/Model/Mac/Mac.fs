namespace Model

open System
open Motsoft.Util
open Model.MacValidation

type Mac =
    private Mac of string

        static member clean (value : string) =
            let validChars = Array.append [| '0'..'9' |] [| 'A'..'F' |]

            value.ToCharArray ()
            |> Array.fold (fun st c -> if validChars |> Array.contains c
                                       then st + c.ToString ()
                                       else st) ""

        static member private canonicalize (value : string) =

            checkNull value

            value
            |> trim
            |> toUpper

        static member private validateTry (value : string) =

            try
                getValidatorsList ()
                |> Array.iter (fun f -> f value)

                value
            with e -> failwith $"Mac: {e.Message}"

        member this.value = let (Mac value) = this in value

        override this.ToString () = this.value

        member this.formatted
            with get () =

                let macString = this.value

                if macString |> String.IsNullOrWhiteSpace
                then ""
                else
                    macString.ToCharArray ()
                    |> Array.splitInto (macString.Length / 2)
                    |> Array.map String
                    |> join "-"

        static member create (value : string) =
            value
            |> Mac.canonicalize
            |> Mac.validateTry
            |> Mac
