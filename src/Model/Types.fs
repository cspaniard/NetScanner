namespace Model

open System
open System.ComponentModel.DataAnnotations
open System.Runtime.InteropServices
open CommandLine

type NameInfo = NameInfo of IpAddress * name : string
type MacInfo = MacInfo of IpAddress * Mac

type DeviceInfo = {
    IpAddress : IpAddress
    Active : bool
    Mac : Mac
    Name : string
}

type ArgLineInfo = ArgLineInfo of paramNames : string * helpText : string

type Parsed = Parsed<ArgumentOptions>
type NotParsed = NotParsed<ArgumentOptions>

type ArgErrors = seq<Error>
type ExceptionErrors = seq<Exception>

type AppErrors =
    | ArgErrors of ArgErrors
    | ExceptionErrors of ExceptionErrors
    | ValidationError of ValidationException

module Definitions =
    let (|LinuxOs|WindowsOs|OtherOs|) _ =
        if RuntimeInformation.IsOSPlatform OSPlatform.Linux then LinuxOs
        else if RuntimeInformation.IsOSPlatform OSPlatform.Windows then WindowsOs
        else OtherOs
