namespace Brokers.Processes.Process

open System.Diagnostics
open System.Threading.Tasks
open Model
open Motsoft.Util

type Broker () =

    //----------------------------------------------------------------------------------------------------
    static member startProcessTry (processName : string) (arguments : string) =

        Process.Start (processName, arguments)
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member startProcessWithStartInfoTry (startInfo : ProcessStartInfo) =

        Process.Start startInfo
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
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

            let processTask = task { do! proc.WaitForExitAsync () } :> Task
            let timeOutTask = task { do! Task.Delay timeOut.value
                                     return Unchecked.defaultof<Process> }

            let! winnerTask = Task.WhenAny [ processTask ; timeOutTask ]

            if winnerTask = timeOutTask then
                proc.Kill ()
                return None
            else
                return Some proc
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member startAndWaitForProcessAsyncTry processName arguments =

        task {
            let proc = Broker.startProcessTry processName arguments
            do! proc.WaitForExitAsync ()

            return proc
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member startProcessAndReadAllLinesAsyncTry processName arguments =

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

            let stdOutLines = ResizeArray<string>()
            let mutable tmpLine = ""

            // Lectura del StdOut
            let! line = proc.StandardOutput.ReadLineAsync ()
            tmpLine <- line

            while tmpLine <> null do
                stdOutLines.Add tmpLine
                let! line = proc.StandardOutput.ReadLineAsync ()
                tmpLine <- line

            // Lectura del StdErr
            let stdErrLines = ResizeArray<string>()
            let! line = proc.StandardError.ReadLineAsync ()
            tmpLine <- line

            while tmpLine <> null do
                stdErrLines.Add tmpLine
                let! line = proc.StandardError.ReadLineAsync ()
                tmpLine <- line

            do! proc.WaitForExitAsync ()

            return (stdOutLines.ToArray (), stdErrLines.ToArray (), proc.ExitCode)
        }
    //----------------------------------------------------------------------------------------------------
