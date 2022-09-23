namespace Model

open System
open CommandLine

type IpInfo =
    IpInfo of IpAddress * active : bool
        static member tuppleOf (ipInfo : IpInfo) = let (IpInfo (address, active)) = ipInfo in (address, active)

type IpInfoMac = IpInfoMac of IpAddress * active : bool * mac : string
type MacInfo = MacInfo of IpAddress * mac : string

type ArgLineInfo = ArgLineInfo of paramNames : string * helpText : string

type Parsed = Parsed<ArgumentOptions>
type NotParsed = NotParsed<ArgumentOptions>

type AppErrors =
    | ArgErrors of seq<Error>
    | ExceptionErrors of seq<Exception>
