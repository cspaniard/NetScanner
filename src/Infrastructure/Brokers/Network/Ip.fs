namespace Brokers.Network.Ip

open System
open System.Diagnostics
open System.Net
open System.Net.NetworkInformation
open System.Runtime.InteropServices
open System.Threading.Tasks
open ArpLookup
open Motsoft.Util

open Model
open Model.Definitions

type private IIProcessBroker = DI.Brokers.ProcessesDI.IProcessBroker

type Broker () =

    static let mutable _pingTimeOut = PingTimeOut.createDefault ()
    static let mutable _retries = Retries.createDefault ()
    static let mutable _nameLookupTimeOut = NameLookupTimeOut.createDefault ()

    //------------------------------------------------------------------------------------------------------------------
    static let getNameForIpLinuxAsync (ipAddress : IpAddress) =

        //--------------------------------------------------------------------------------------------------------------
        let processProcInfo (proc : Process) =

            backgroundTask {

                if proc.ExitCode <> 0 then
                    return NameInfo (ipAddress, "")
                else
                    let! result = proc.StandardOutput.ReadToEndAsync ()
                    let hostFullName = (result |> split "=")[1] |> trim
                    let hostName = (hostFullName |> split ".")[0]
                    return NameInfo (ipAddress, hostName)
            }
        //--------------------------------------------------------------------------------------------------------------

        backgroundTask {

            let newProcessTask =
                IIProcessBroker.startProcessWithTimeOutAsync "nslookup"
                                                             Broker.NameLookupTimeOut.timeOut
                                                             ipAddress.value

            match! newProcessTask with
            | Some proc -> return! processProcInfo proc
            | None -> return NameInfo (ipAddress, "")
        }
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static let getNameForIpWindowsAsync (ipAddress : IpAddress) =

        backgroundTask {

            let args = $"-n 1 -a -w {Broker.PingTimeOut} {ipAddress}"

            match! IIProcessBroker.startProcessWithTimeOutAsync "ping" Broker.NameLookupTimeOut.timeOut args with
            | Some proc ->
                let! result = proc.StandardOutput.ReadToEndAsync ()
                let hostName = result |> split "[" |> Array.item 0 |> split " " |> Array.last
                return NameInfo (ipAddress, hostName)
            | None ->
                return NameInfo (ipAddress, "")
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
            let! physicalAddress = IPAddress.Parse ipAddress.value
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
    static member getNameInfoForIpAsyncTry (ip : IpAddress) =

        match RuntimeInformation.OSDescription with
        | LinuxOs -> getNameForIpLinuxAsync ip
        | WindowsOs -> getNameForIpWindowsAsync ip
        | OtherOs ->  NameInfo (ip, "") |> Task.FromResult
    //------------------------------------------------------------------------------------------------------------------
