namespace Model

open Model.TimeOutValidation

type TimeOut =
    private TimeOut of int
        static member private validateTry (value : int) =

            getValidatorsList ()
            |> Array.iter (fun f -> f value)

            value

        member this.value = let (TimeOut value) = this in value
        override this.ToString() = this.value.ToString()

        static member create value =
            value
            |> TimeOut.validateTry
            |> TimeOut
