namespace Brokers.Network.Ip

open System.Net
open System.Net.NetworkInformation
open ArpLookup
open Motsoft.Util

open Model
open Brokers.Network.Ip.Exceptions

type Broker () =

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

            return (ipAddress, (resultStatus = IPStatus.Success)) |> IpInfo
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
    static member getIpV4NetworkClassesAsyncTry () =

        backgroundTask {
            let! allIpAddresses = Dns.GetHostAddressesAsync(Dns.GetHostName())

            let ipV4Addresses =
                allIpAddresses
                |> Array.filter (fun address -> address.GetAddressBytes().Length = 4)

            ipV4Addresses.Length = 0 |> failWithIfTrue IP_NO_NETWORKS_FOUND

            return ipV4Addresses
                   |> Array.map (fun address ->
                                     address.ToString()
                                     |> split "."
                                     |> Array.take 3
                                     |> Array.fold (fun st s -> st + s + ".") "")
        }
    //----------------------------------------------------------------------------------------------------
