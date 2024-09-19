namespace Brokers

open System.Net
open System.Net.NetworkInformation
open System.Threading
open ArpLookup
open DI.Interfaces
open Motsoft.Util

open Model
open Model.Definitions
open Model.Constants

module private IpBrokerExceptions =

    let [<Literal>] OS_UNSUPPORTED = "Sistema Operativo no soportado."

open IpBrokerExceptions

type IpBroker (ProcessBroker : IProcessBroker, pingTimeOut : PingTimeOut,
               retries : Retries, nameLookupTimeOut : NameLookupTimeOut) =

    //------------------------------------------------------------------------------------------------------------------
    let startNameLookupProcessAsyncTry = ProcessBroker.startProcessReadLinesAsyncTry nameLookupTimeOut.timeOut
    let startProcessReadLinesNoTimeOutAsyncTry = ProcessBroker.startProcessReadLinesAsyncTry (TimeOut.create 0)

    let lookUpApp =
        match OS_PLATFORM with
        | LinuxOs -> "nslookup"
        | WindowsOs -> "ping"
        | MacOs -> "nslookup"
        | OtherOs -> failwith OS_UNSUPPORTED
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    let getNameInfoFromProcessAsyncTry args =

        backgroundTask {
            return! startNameLookupProcessAsyncTry lookUpApp args
        }
    //------------------------------------------------------------------------------------------------------------------

    interface IIpBroker with
        //--------------------------------------------------------------------------------------------------------------
        member _.getDeviceInfoStatusForIpAsync (ipAddress : IpAddress) =

            let createDeviceInfo active ttl =
                { IpAddress = ipAddress
                  Active = active
                  Mac = Mac.create ""
                  Name = ""
                  Ttl = ttl }

            let rec doPing (ping: Ping) retriesLeft =

                backgroundTask {

                    if retriesLeft = 0 then
                        return None
                    else
                        let! pingReply = ping.SendPingAsync(ipAddress.value, pingTimeOut.value)

                        if pingReply.Status = IPStatus.Success then
                            return Some pingReply
                        else
                            return! doPing ping (retriesLeft - 1)
                }

            backgroundTask {

                try
                    use ping = new Ping()
                    let! pingReply = doPing ping retries.value

                    match pingReply with
                    | Some reply ->
                        let ttlOption = reply.Options |> Option.ofObj |> Option.map _.Ttl
                        return createDeviceInfo true ttlOption
                    | None ->
                        return createDeviceInfo false None
                with _ ->
                    return createDeviceInfo false None
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
            let processLinuxRawDataAsync (stdOutLines : string []) =

                try
                    let result =
                        stdOutLines
                        |> Array.skipWhile System.String.IsNullOrWhiteSpace
                        |> Array.head

                    let hostName = (result |> split "=")[1] |> trim |> split "." |> Array.head

                    NameInfo (ipAddress, hostName)
                with _ ->
                    emptyNameInfo
            //----------------------------------------------------------------------------------------------------------

            //----------------------------------------------------------------------------------------------------------
            let processWindowsRawDataAsync (stdOutLines : string []) =

                try
                    let result =
                        stdOutLines
                        |> Array.skipWhile System.String.IsNullOrWhiteSpace
                        |> Array.head

                    let hostName =
                        if result.Contains "[" then
                            (result |> split "[")[0] |> split " " |> Array.last
                        else
                            ""

                    NameInfo (ipAddress, hostName)
                with _ ->
                    emptyNameInfo
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
                    with _ ->
                        return emptyNameInfo
                }
            // ---------------------------------------------------------------------------------------------------------

            let args, processRawDataFun =
                match OS_PLATFORM with
                | LinuxOs -> ipAddress.value, processLinuxRawDataAsync
                | WindowsOs -> $"-n 1 -a -w {pingTimeOut} {ipAddress}", processWindowsRawDataAsync
                | MacOs | OtherOs -> failwith OS_UNSUPPORTED


            backgroundTask {
                if useDns then
                    return! getNameInfoFromDnsAsync ipAddress
                else
                    match! getNameInfoFromProcessAsyncTry args with
                    | stdOutLines, _, exitCode when exitCode = 0 -> return processRawDataFun stdOutLines
                    | _ -> return emptyNameInfo
            }
        //--------------------------------------------------------------------------------------------------------------
