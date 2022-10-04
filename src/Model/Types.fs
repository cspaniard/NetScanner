namespace Model

open System
open CommandLine

type IpStatus = IpStatus of IpAddress * active : bool
type NameInfo = NameInfo of IpAddress * name : string
type MacInfo = MacInfo of IpAddress * Mac

type DeviceInfo = DeviceInfo of IpAddress * active : bool * Mac * name : string

type ArgLineInfo = ArgLineInfo of paramNames : string * helpText : string

type Parsed = Parsed<ArgumentOptions>
type NotParsed = NotParsed<ArgumentOptions>

type ArgErrors = seq<Error>
type ExceptionErrors = seq<Exception>

type AppErrors =
    | ArgErrors of ArgErrors
    | ExceptionErrors of ExceptionErrors
