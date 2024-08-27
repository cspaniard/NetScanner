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
    Ttl : int option
}

type ArgLineInfo = ArgLineInfo of paramNames : string * helpText : string

type Parsed = Parsed<ArgumentOptions>
type NotParsed = NotParsed<ArgumentOptions>

type ArgErrors = seq<Error>
type ExceptionErrors = seq<Exception>
type ValidationErrors = seq<ValidationException>

type AppErrors =
    | ArgErrors of ArgErrors
    | ExceptionErrors of ExceptionErrors
    | ValidationError of ValidationException
    | ValidationErrors of ValidationErrors


module Definitions =

    let (|Parsed|NotParsed|) (parserResult : ParserResult<_>) =

        match parserResult with
        | :? Parsed -> Parsed
        | :? NotParsed -> NotParsed
        | _ -> failwith "No debiéramos llegar aquí."


    let (|LinuxOs|WindowsOs|MacOs|OtherOs|) _ =

        let knownOsList =
            [ (OSPlatform.Linux, LinuxOs)
              (OSPlatform.Windows, WindowsOs)
              (OSPlatform.OSX, MacOs) ]

        match knownOsList |> List.tryFind (fst >> RuntimeInformation.IsOSPlatform) with
        | Some (_, os) -> os
        | None -> OtherOs
