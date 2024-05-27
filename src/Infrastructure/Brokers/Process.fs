namespace Brokers

open System
open System.Diagnostics
open System.IO
open System.Threading
open Model
open DI.Interfaces

type ProcessBroker () as this =

    // -----------------------------------------------------------------------------------------------------------------
    let self = this :> IProcessBroker
    // -----------------------------------------------------------------------------------------------------------------

    interface IProcessBroker with
        //--------------------------------------------------------------------------------------------------------------
        member _.startProcessTry (processName : string) (arguments : string) =

            Process.Start (processName, arguments)
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        member _.startProcessWithStartInfoTry (startInfo : ProcessStartInfo) =

            Process.Start startInfo
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        member _.startProcessWithTimeOutAsync (processName : string) (timeOut : TimeOut) (arguments : string) =

            let startInfo = ProcessStartInfo (RedirectStandardOutput = true,
                                              RedirectStandardError = true,
                                              FileName = processName,
                                              Arguments = arguments,
                                              WindowStyle = ProcessWindowStyle.Hidden,
                                              UseShellExecute = false,
                                              CreateNoWindow = true)

            backgroundTask {

                let proc = self.startProcessWithStartInfoTry startInfo

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
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        member _.startAndWaitForProcessAsyncTry (processName : string) (arguments : string) =

            backgroundTask {
                let proc = self.startProcessTry processName arguments
                do! proc.WaitForExitAsync ()

                return proc
            }
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        member _.startProcessAndReadAllLinesAsyncTry (processName : string) (arguments : string) =

            //----------------------------------------------------------------------------------------------------------
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
            //----------------------------------------------------------------------------------------------------------

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
        //--------------------------------------------------------------------------------------------------------------
