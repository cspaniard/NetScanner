namespace Brokers.Processes.Process

open System
open System.Diagnostics
open System.IO
open System.Threading
open Model

type Broker () =

    //------------------------------------------------------------------------------------------------------------------
    static member startProcessTry (processName : string) (arguments : string) =

        Process.Start (processName, arguments)
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member startProcessWithStartInfoTry (startInfo : ProcessStartInfo) =

        Process.Start startInfo
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member startProcessWithTimeOutAsync processName (timeOut : TimeOut) arguments =

        let startInfo = ProcessStartInfo (RedirectStandardOutput = true,
                                          RedirectStandardError = true,
                                          FileName = processName,
                                          Arguments = arguments,
                                          WindowStyle = ProcessWindowStyle.Hidden,
                                          UseShellExecute = false,
                                          CreateNoWindow = true)

        backgroundTask {

            let proc = Broker.startProcessWithStartInfoTry startInfo

            let cts = new CancellationTokenSource ()

            if timeOut.value > 0 then
                cts.CancelAfter timeOut.value

            try
                do! proc.WaitForExitAsync cts.Token
                return Some proc
            with
            | :? OperationCanceledException ->
                proc.Kill ()
                return None
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member startAndWaitForProcessAsyncTry processName arguments =

        task {
            let proc = Broker.startProcessTry processName arguments
            do! proc.WaitForExitAsync ()

            return proc
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member startProcessAndReadAllLinesAsyncTry processName arguments =

        //--------------------------------------------------------------------------------------------------------------
        let readAllLinesAsyncTry (reader : StreamReader) =

            let rec readAllLinesAsync (reader : StreamReader) (stdOutLines: ResizeArray<string>) =
                task {
                    match! reader.ReadLineAsync () with
                    | null -> return stdOutLines
                    | line ->
                        stdOutLines.Add line
                        return! readAllLinesAsync reader stdOutLines
                }

            readAllLinesAsync reader (ResizeArray<string>())
        //--------------------------------------------------------------------------------------------------------------

        backgroundTask {
            let proc =
                ProcessStartInfo (RedirectStandardOutput = true,
                                  RedirectStandardError = true,
                                  FileName = processName,
                                  WindowStyle = ProcessWindowStyle.Hidden,
                                  CreateNoWindow = true,
                                  UseShellExecute = false,
                                  Arguments = arguments)
                |> Process.Start

            let! stdOutLines = readAllLinesAsyncTry proc.StandardOutput
            let! stdErrLines = readAllLinesAsyncTry proc.StandardError

            do! proc.WaitForExitAsync ()

            return (stdOutLines.ToArray (), stdErrLines.ToArray (), proc.ExitCode)
        }
    //------------------------------------------------------------------------------------------------------------------
