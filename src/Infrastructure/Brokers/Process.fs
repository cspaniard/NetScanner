namespace Brokers

open System
open System.Diagnostics
open System.IO
open System.Threading
open System.Threading.Tasks
open Model
open DI.Interfaces

type ProcessBroker () =

    interface IProcessBroker with

        //--------------------------------------------------------------------------------------------------------------
        member _.startProcessWithTimeOutAsync (processName : string) (timeOut : TimeOut) (arguments : string) =

            let startInfo = ProcessStartInfo (RedirectStandardOutput = true,
                                              RedirectStandardError = true,
                                              FileName = processName,
                                              Arguments = arguments,
                                              WindowStyle = ProcessWindowStyle.Hidden,
                                              CreateNoWindow = true,
                                              UseShellExecute = false)

            backgroundTask {

                let proc = Process.Start startInfo

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
        member _.startProcessAndReadAllLinesAsyncTry (processName : string) (arguments : string) =

            //----------------------------------------------------------------------------------------------------------
            let readAllLinesAsyncTry (reader : StreamReader) =

                backgroundTask {
                    let lines = ResizeArray<string> ()

                    let mutable shouldContinue = true

                    while shouldContinue do
                        match! reader.ReadLineAsync () with
                        | null -> shouldContinue <- false
                        | line -> lines.Add line

                    return lines.ToArray()
                }
            //----------------------------------------------------------------------------------------------------------

            backgroundTask {
                let proc =
                    ProcessStartInfo (RedirectStandardOutput = true,
                                      RedirectStandardError = true,
                                      FileName = processName,
                                      Arguments = arguments,
                                      WindowStyle = ProcessWindowStyle.Hidden,
                                      CreateNoWindow = true,
                                      UseShellExecute = false)
                    |> Process.Start

                let! results =
                    Task.WhenAll [| readAllLinesAsyncTry proc.StandardOutput
                                    readAllLinesAsyncTry proc.StandardError |]

                let stdOutLines, stdErrLines = results[0], results[1]

                do! proc.WaitForExitAsync ()

                return (stdOutLines, stdErrLines, proc.ExitCode)
            }
        //--------------------------------------------------------------------------------------------------------------
