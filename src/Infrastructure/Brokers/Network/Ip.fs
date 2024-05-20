namespace Brokers.Network.Ip

open System
open System.Diagnostics
open System.Net
open System.Net.NetworkInformation
open System.Runtime.InteropServices
open System.Threading
open ArpLookup
open Motsoft.Util

open Model
open Model.Definitions

open Brokers.Network.Ip.Exceptions

type private IIProcessBroker = DI.Brokers.ProcessesDI.IProcessBroker

type Broker () =

    //------------------------------------------------------------------------------------------------------------------
    static let mutable _pingTimeOut = PingTimeOut.createDefault ()
    static let mutable _retries = Retries.createDefault ()
    static let mutable _nameLookupTimeOut = NameLookupTimeOut.createDefault ()

    static let lookUpApp =
        match RuntimeInformation.OSDescription with
        | LinuxOs -> "nslookup"
        | WindowsOs -> "ping"
        | MacOs -> "nslookup"
        | OtherOs -> failwith OS_UNSUPPORTED
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static let startProcessGetNameInfoForIpAsyncTry args =

        backgroundTask {
            return! IIProcessBroker.startProcessWithTimeOutAsync lookUpApp Broker.NameLookupTimeOut.timeOut args
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member init pingTimeOut retries nameLookupTimeOut =

        _pingTimeOut <- pingTimeOut
        _retries <- retries
        _nameLookupTimeOut <- nameLookupTimeOut

        Arp.LinuxPingTimeout <- TimeSpan.FromMilliseconds Broker.PingTimeOut.value
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member PingTimeOut with get () : PingTimeOut = _pingTimeOut
    static member Retries with get () : Retries = _retries
    static member NameLookupTimeOut with get () : NameLookupTimeOut = _nameLookupTimeOut
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member getDeviceInfoStatusForIpAsync (ipAddress: IpAddress) =

        backgroundTask {

            let ping = new Ping ()
            let mutable retryCount = Broker.Retries.value
            let mutable resultStatus = IPStatus.Unknown

            while retryCount > 0 do
                let! pingReply = ping.SendPingAsync (ipAddress.value, Broker.PingTimeOut.value)

                if pingReply.Status = IPStatus.Success then
                    resultStatus <- pingReply.Status
                    retryCount <- 0
                else
                    retryCount <- retryCount - 1

            return ({ IpAddress = ipAddress
                      Active = (resultStatus = IPStatus.Success)
                      Mac = Mac.create ""
                      Name = ""} : DeviceInfo)
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member getMacForIpAsync (ipAddress : IpAddress) =

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
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member getNameInfoForIpAsyncTry useDns (ipAddress : IpAddress) =

        let emptyNameInfo = NameInfo (ipAddress, "")

        //--------------------------------------------------------------------------------------------------------------
        let processLinuxProcInfoAsyncTry (proc : Process) =

            backgroundTask {

                if proc.ExitCode <> 0 then
                    return emptyNameInfo
                else
                    let! result = proc.StandardOutput.ReadToEndAsync ()
                    let hostName = ((result |> split "=")[1] |> trim |> split ".")[0]

                    return NameInfo (ipAddress, hostName)
            }
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
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
        //--------------------------------------------------------------------------------------------------------------

        // -------------------------------------------------------------------------------------------------------------
        let getNameInfoFromDnsAsync (ipAdress : IpAddress) =

            backgroundTask {
                let cts = new CancellationTokenSource ()

                if Broker.NameLookupTimeOut.value > 0 then
                    cts.CancelAfter Broker.NameLookupTimeOut.value

                try
                    let! ipHostEntry = Dns.GetHostEntryAsync (ipAdress.value, cts.Token)
                    return NameInfo (ipAddress, ipHostEntry.HostName)
                with _ -> return emptyNameInfo
            }
        // -------------------------------------------------------------------------------------------------------------

        let args, processProcInfoFun =
            match RuntimeInformation.OSDescription with
            | LinuxOs -> ipAddress.value, processLinuxProcInfoAsyncTry
            | WindowsOs -> $"-n 1 -a -w {Broker.PingTimeOut} {ipAddress}", processWindowsProcInfoAsyncTry
            | MacOs | OtherOs -> failwith OS_UNSUPPORTED


        backgroundTask {
            if useDns then
                return! getNameInfoFromDnsAsync ipAddress
            else
                match! startProcessGetNameInfoForIpAsyncTry args with
                | Some proc -> return! processProcInfoFun proc
                | None -> return emptyNameInfo
        }
    //------------------------------------------------------------------------------------------------------------------
