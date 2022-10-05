namespace Brokers.Network.Ip

open System.Diagnostics
open System.Net
open System.Net.NetworkInformation
open System.Runtime.InteropServices
open ArpLookup
open Motsoft.Util

open Model
open Model.Definitions

type private IIProcessBroker = DI.Brokers.ProcessesDI.IProcessBroker

type Broker () =

    //----------------------------------------------------------------------------------------------------
    static let getNameForIpLinuxAsync (timeOut : TimeOut) (ipAddress : IpAddress) =

        let processProcInfo (proc : Process) =

            backgroundTask {

                if proc.ExitCode <> 0 then
                    return NameInfo (ipAddress, "")
                else
                    let! result = proc.StandardOutput.ReadToEndAsync()
                    let hostFullName = (result |> split "=")[1] |> trim
                    let hostName = (hostFullName |> split ".")[0]
                    return NameInfo (ipAddress, hostName)
            }

        backgroundTask {

            let args = $"{ipAddress}"

            match! IIProcessBroker.startProcessWithTimeOutAsync "nslookup" timeOut args with
            | Some proc -> return! processProcInfo proc
            | None -> return NameInfo (ipAddress, "")
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let getNameForIpWindowsAsync (timeOut : TimeOut) (ipAddress : IpAddress) =

        backgroundTask {

            let args = $"-n 1 -a -w {timeOut} {ipAddress}"

            match! IIProcessBroker.startProcessWithTimeOutAsync "ping" timeOut args with
            | Some proc ->
                let! result = proc.StandardOutput.ReadToEndAsync()
                let hostName = result |> split "[" |> Array.item 0 |> split " " |> Array.last
                return NameInfo (ipAddress, hostName)
            | None ->
                return NameInfo (ipAddress, "")
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member getDeviceInfoStatus (timeOut : TimeOut) (retries : Retries) (ipAddress: IpAddress) =

        backgroundTask {

            let ping = new Ping()
            let mutable retryCount : int = retries.value
            let mutable resultStatus = IPStatus.Unknown

            while retryCount > 0 do
                let! pingReply = ping.SendPingAsync(ipAddress.value, timeOut.value)

                if pingReply.Status = IPStatus.Success then
                    resultStatus <- pingReply.Status
                    retryCount <- 0
                else
                    retryCount <- retryCount - 1

            return DeviceInfo (ipAddress, (resultStatus = IPStatus.Success), Mac.create "", "")
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member getMacForIpAsync (ipAddress: IpAddress) =

        backgroundTask {
            let! physicalAddress = Arp.LookupAsync(IPAddress.Parse(ipAddress.value))

            if physicalAddress <> null &&
                physicalAddress.GetAddressBytes() <> Array.zeroCreate (physicalAddress.GetAddressBytes()).Length
            then
                return MacInfo (ipAddress, Mac.create (physicalAddress.ToString()))
            else
                return MacInfo (ipAddress, Mac.create "")
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member getNameInfoForIpAsyncTry (timeOut : TimeOut) (ip : IpAddress) =

        match RuntimeInformation.OSArchitecture with
        | LinuxOs -> getNameForIpLinuxAsync timeOut ip
        | WindowsOs -> getNameForIpWindowsAsync timeOut ip
        | OtherOs -> backgroundTask { return NameInfo (ip, "") }
    //----------------------------------------------------------------------------------------------------
