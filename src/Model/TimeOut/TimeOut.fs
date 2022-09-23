namespace NetScanner.Model

open NetScanner.Model.TimeOutValidation

type TimeOut =
    private TimeOut of int
        static member private validate (value : int) =

            try
                getValidatorsList ()
                |> Array.iter (fun f -> f value)

                value
            with e -> failwith $"TimeOut: {e.Message}"

        member this.value = let (TimeOut value) = this in value
        override this.ToString() = this.value.ToString()

        static member create value =
            value
            |> TimeOut.validate
            |> TimeOut
