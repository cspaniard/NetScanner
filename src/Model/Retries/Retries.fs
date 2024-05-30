namespace Model

open System.ComponentModel.DataAnnotations
open Model.Constants
open Model.RetriesValidation

type Retries =
    private Retries of int
        static member private validateTry (value : int) =

            try
                getValidatorsList ()
                |> Array.iter (fun f -> f value)

                value
            with e -> raise <| ValidationException $"reintentos: {e.Message}"

        member this.value = let (Retries value) = this in value
        override this.ToString () = this.value.ToString ()

        static member create value =
            value
            |> Retries.validateTry
            |> Retries

        static member createDefault () =
            Retries.create DEF_RETRIES
