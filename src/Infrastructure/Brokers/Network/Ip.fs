namespace Brokers.Network.Ip

open System.Net
open System.Net.NetworkInformation
open ArpLookup

open Model

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

            return IpInfo (ipAddress, (resultStatus = IPStatus.Success))
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
