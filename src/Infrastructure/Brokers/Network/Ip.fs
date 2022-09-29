namespace Brokers.Network.Ip

open System.Diagnostics
open System.Net
open System.Net.NetworkInformation
open System.Runtime.InteropServices
open System.Threading.Tasks
open ArpLookup
open Motsoft.Util

open Model
open Model.Definitions

type private IIProcessBroker = Infrastructure.DI.Brokers.ProcessesDI.IProcessBroker

type Broker () =

    //----------------------------------------------------------------------------------------------------
    static let getNameForIpLinux (nameLookUpTimeOut : TimeOut) (ip : IpAddress) =
        backgroundTask {
            return NameInfo.NameInfo (ip, "dummy-dev-name") |> IpInfo.NameInfo
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static let getNameForIpWindows (nameLookUpTimeOut : TimeOut) (ip : IpAddress) =

        let startInfo = ProcessStartInfo(RedirectStandardOutput = true,
                                         RedirectStandardError = true,
                                         FileName = "ping",
                                         Arguments = $"-n 1 -a -w 500 {ip}",
                                         WindowStyle = ProcessWindowStyle.Hidden,
                                         UseShellExecute = false,
                                         CreateNoWindow = true)

        backgroundTask {

            let proc = IIProcessBroker.startProcessWithStartInfoTry startInfo

            let pingTask = task { do! proc.WaitForExitAsync() } :> Task
            let timeOutTask = task { do! Task.Delay(nameLookUpTimeOut.value)
                                     return Unchecked.defaultof<Process> }

            let! winnerTask = Task.WhenAny [ pingTask ; timeOutTask ]

            if winnerTask = timeOutTask then
                proc.Kill()
                return NameInfo.NameInfo (ip, "") |> IpInfo.NameInfo
            else
                let! result = proc.StandardOutput.ReadToEndAsync()
                let hostName = result |> split "[" |> Array.item 0 |> split " " |> Array.last
                return NameInfo.NameInfo (ip, hostName) |> IpInfo.NameInfo
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member pingIpAsync (timeOut : TimeOut) (retries : Retries) (ipAddress: IpAddress) =

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

            return IpStatus.IpStatus (ipAddress, (resultStatus = IPStatus.Success)) |> IpInfo.IpStatus
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member getMacForIpAsync (ipAddress: IpAddress) =

        backgroundTask {
            let! physicalAddress = Arp.LookupAsync(IPAddress.Parse(ipAddress.value))

            if physicalAddress <> null &&
               physicalAddress.GetAddressBytes() <> Array.zeroCreate (physicalAddress.GetAddressBytes()).Length
            then
                return MacInfo (ipAddress, physicalAddress.ToString())
            else
                return MacInfo (ipAddress, "")
        }
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------
    static member getNameInfoForIpAsyncTry (timeOut : TimeOut) (ip : IpAddress) =

        match RuntimeInformation.OSArchitecture with
        | LinuxOs -> getNameForIpLinux timeOut ip
        | WindowsOs -> getNameForIpWindows timeOut ip
        | OtherOs -> backgroundTask { return NameInfo.NameInfo (ip, "") |> IpInfo.NameInfo}
    //----------------------------------------------------------------------------------------------------
