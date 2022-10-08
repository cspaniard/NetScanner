namespace Model

open System.ComponentModel.DataAnnotations
open Constants

type NameLookupTimeOut =
    private NameLookupTimeOut of TimeOut

        static member create value =
            try
                value
                |> TimeOut.create
                |> NameLookupTimeOut
            with e -> raise <| ValidationException $"nombres-timeout: {e.Message}"

        static member createDefault () = NameLookupTimeOut.create DEF_NAME_LOOKUP_TIMEOUT

        member this.timeOut = let (NameLookupTimeOut timeOut) = this in timeOut

        member this.value = let timeOut = this.timeOut
                            let (TimeOut value) = timeOut in value

        override this.ToString () = this.value.ToString ()
