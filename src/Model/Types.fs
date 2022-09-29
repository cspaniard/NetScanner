namespace Model

open System
open System.Runtime.InteropServices
open CommandLine

type IpStatus = IpStatus of IpAddress * active : bool
type NameInfo = NameInfo of IpAddress * name : string

type IpInfo =
    | IpStatus of IpStatus
    | NameInfo of NameInfo

type DeviceInfo = DeviceInfo of IpAddress * active : bool * mac : string * name : string
type MacInfo = MacInfo of IpAddress * mac : string

type ArgLineInfo = ArgLineInfo of paramNames : string * helpText : string

type Parsed = Parsed<ArgumentOptions>
type NotParsed = NotParsed<ArgumentOptions>

type ArgErrors = seq<Error>
type ExceptionErrors = seq<Exception>

type AppErrors =
    | ArgErrors of ArgErrors
    | ExceptionErrors of ExceptionErrors


module Definitions =

    let (|LinuxOs|WindowsOs|OtherOs|) _ =
        if RuntimeInformation.IsOSPlatform(OSPlatform.Linux) then LinuxOs
        else if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then WindowsOs
        else OtherOs
