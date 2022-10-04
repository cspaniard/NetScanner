module Model.Definitions

open Model
open System.Runtime.InteropServices

let (|LinuxOs|WindowsOs|OtherOs|) _ =
    if RuntimeInformation.IsOSPlatform(OSPlatform.Linux) then LinuxOs
    else if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then WindowsOs
    else OtherOs

type ScanNetworkParams = {
    PingTimeOut : TimeOut
    Retries : Retries
    ShowMacs : bool
    ShowNames : bool
    NameLookUpTimeOut : TimeOut
    Network : IpNetwork
}

type OutputDeviceInfosParams = {
    ActivesOnly : bool
    Separator : string
    ShowMacs : bool
    ShowNames : bool
    DeviceInfos : DeviceInfoArray
}
