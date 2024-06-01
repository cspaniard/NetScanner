namespace Brokers

open System.Diagnostics
open System.Net
open System.Net.NetworkInformation
open System.Runtime.InteropServices
open System.Threading
open ArpLookup
open DI.Interfaces
open Motsoft.Util

open Model
open Model.Definitions

module private IpBrokerExceptions =

    let [<Literal>] OS_UNSUPPORTED = "Sistema Operativo no soportado."

open IpBrokerExceptions

type IpBroker (ProcessBroker : IProcessBroker, pingTimeOut : PingTimeOut,
               retries : Retries, nameLookupTimeOut : NameLookupTimeOut) =

    //------------------------------------------------------------------------------------------------------------------
    let startProcessAsyncTry = ProcessBroker.startProcessAsyncTry nameLookupTimeOut.timeOut
    let startProcessReadLinesNoTimeOutAsyncTry = ProcessBroker.startProcessReadLinesAsyncTry (TimeOut.create 0)

    let lookUpApp =
        match RuntimeInformation.OSDescription with
        | LinuxOs -> "nslookup"
        | WindowsOs -> "ping"
        | MacOs -> "nslookup"
        | OtherOs -> failwith OS_UNSUPPORTED
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    let startProcessGetNameInfoForIpAsyncTry args =

        backgroundTask {
            return! startProcessAsyncTry lookUpApp args
        }
    //------------------------------------------------------------------------------------------------------------------

    interface IIpBroker with
        //--------------------------------------------------------------------------------------------------------------
        member _.getDeviceInfoStatusForIpAsync (ipAddress: IpAddress) =

            backgroundTask {

                try
                    let ping = new Ping ()
                    let mutable retryCount = retries.value
                    let mutable resultStatus = IPStatus.Unknown

                    while retryCount > 0 do
                        let! pingReply = ping.SendPingAsync (ipAddress.value, pingTimeOut.value)

                        if pingReply.Status = IPStatus.Success then
                            resultStatus <- pingReply.Status
                            retryCount <- 0
                        else
                            retryCount <- retryCount - 1

                    return ({ IpAddress = ipAddress
                              Active = (resultStatus = IPStatus.Success)
                              Mac = Mac.create ""
                              Name = ""} : DeviceInfo)
                with _ ->
                    return ({ IpAddress = ipAddress
                              Active = false
                              Mac = Mac.create ""
                              Name = ""} : DeviceInfo)
            }
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        member _.getMacForIpAsync (ipAddress : IpAddress) =

            backgroundTask {

                let! physicalAddress = ipAddress.value
                                       |> IPAddress.Parse
                                       |> Arp.LookupAsync

                if physicalAddress <> null &&
                    physicalAddress.GetAddressBytes () <> Array.zeroCreate (physicalAddress.GetAddressBytes ()).Length
                then
                    return MacInfo (ipAddress, Mac.create (physicalAddress.ToString ()))
                else
                    return MacInfo (ipAddress, Mac.create "")
            }
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        member _.getLocalMacInfoForIpAsyncTry (ipAddress : IpAddress) =

            //----------------------------------------------------------------------------------------------------------
            let getMacInfoOptionForIp (ipAddress : IpAddress) (stdOutLines : string []) =
                [|
                    for i, line in stdOutLines |> Array.mapi (fun i v -> i, v) do
                        if line.Contains $"inet {ipAddress.value}" then
                            let macValue = (stdOutLines[i - 1] |> split " ")[1]
                            yield MacInfo(ipAddress, Mac.create macValue)
                |]
                |> Array.tryFind (fun (MacInfo (ipAddress, _)) -> ipAddress = ipAddress)
            //----------------------------------------------------------------------------------------------------------

            backgroundTask {
                let! stdOutLines, _, _ = startProcessReadLinesNoTimeOutAsyncTry "ip" "a"

                return
                    match stdOutLines |> getMacInfoOptionForIp ipAddress with
                    | Some macInfo -> macInfo
                    | None -> MacInfo (ipAddress, Mac.create "")
            }
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        member _.getNameInfoForIpAsyncTry useDns (ipAddress : IpAddress) =

            let emptyNameInfo = NameInfo (ipAddress, "")

            //----------------------------------------------------------------------------------------------------------
            let processLinuxProcInfoAsyncTry (proc : Process) =

                backgroundTask {

                    if proc.ExitCode <> 0 then
                        return emptyNameInfo
                    else
                        let! result = proc.StandardOutput.ReadToEndAsync ()
                        let hostName = ((result |> split "=")[1] |> trim |> split ".")[0]

                        return NameInfo (ipAddress, hostName)
                }
            //----------------------------------------------------------------------------------------------------------

            //----------------------------------------------------------------------------------------------------------
            let processWindowsProcInfoAsyncTry (proc : Process) =

                backgroundTask {

                    let! result = proc.StandardOutput.ReadToEndAsync ()

                    let hostName =
                        if result.Contains "[" then
                            (result |> split "[")[0] |> split " " |> Array.last
                        else
                            ""

                    return NameInfo (ipAddress, hostName)
                }
            //----------------------------------------------------------------------------------------------------------

            // ---------------------------------------------------------------------------------------------------------
            let getNameInfoFromDnsAsync (ipAdress : IpAddress) =

                backgroundTask {
                    let cts = new CancellationTokenSource ()

                    if nameLookupTimeOut.value > 0 then
                        cts.CancelAfter nameLookupTimeOut.value

                    try
                        let! ipHostEntry = Dns.GetHostEntryAsync (ipAdress.value, cts.Token)
                        return NameInfo (ipAddress, ipHostEntry.HostName)
                    with _ -> return emptyNameInfo
                }
            // ---------------------------------------------------------------------------------------------------------

            let args, processProcInfoFun =
                match RuntimeInformation.OSDescription with
                | LinuxOs -> ipAddress.value, processLinuxProcInfoAsyncTry
                | WindowsOs -> $"-n 1 -a -w {pingTimeOut} {ipAddress}", processWindowsProcInfoAsyncTry
                | MacOs | OtherOs -> failwith OS_UNSUPPORTED


            backgroundTask {
                if useDns then
                    return! getNameInfoFromDnsAsync ipAddress
                else
                    match! startProcessGetNameInfoForIpAsyncTry args with
                    | Some proc -> return! processProcInfoFun proc
                    | None -> return emptyNameInfo
            }
        //--------------------------------------------------------------------------------------------------------------
