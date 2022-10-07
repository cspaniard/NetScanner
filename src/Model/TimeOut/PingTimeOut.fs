namespace Model

open System.ComponentModel.DataAnnotations
open Constants

type PingTimeOut =
    private PingTimeOut of TimeOut

        static member create value =
            try
                value
                |> TimeOut.create
                |> PingTimeOut
            with e -> raise (ValidationException($"ping-timeout: {e.Message}"))

        static member createDefault () = PingTimeOut.create DEF_PING_TIMEOUT

        member this.timeOut = let (PingTimeOut timeOut) = this in timeOut

        member this.value = let timeOut = this.timeOut
                            let (TimeOut value) = timeOut in value

        override this.ToString() = this.value.ToString()
