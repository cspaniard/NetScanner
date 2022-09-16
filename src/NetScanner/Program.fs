
open System
open System.Diagnostics
open System.Threading.Tasks

type private IIpService = Infrastructure.DI.Services.NetworkDI.IIpService


let myTask =
    task {
        let stopwatch = Stopwatch.StartNew()
        let! ipsInfo = IIpService.getAllIpStatusInNetworkAsyncTry 500 3 "192.168.1."
        stopwatch.Stop()

        ipsInfo
        |> Array.iter (fun (ip, status) -> Console.WriteLine $"%s{ip}\t%b{status}" )

        Console.WriteLine $"\nTiempo: {stopwatch.ElapsedMilliseconds}ms"
    } :> Task

myTask.Wait()
