module DI.Interfaces

open System
open System.Diagnostics
open System.Threading.Tasks
open CommandLine
open Model

//----------------------------------------------------------------------------------------------------------------------
type IProcessBroker =
    abstract member startProcessReadLinesAsyncTry: timeOut : TimeOut -> processName: string -> arguments: string ->
                                                   Task<string array * string array * int>

type IIpBroker =
    abstract member getDeviceInfoStatusForIpAsync: ipAddress: IpAddress -> Task<DeviceInfo>
    abstract member getMacForIpAsync: ipAddress : IpAddress -> Task<MacInfo>
    abstract member getLocalMacInfoForIpAsyncTry: ipAddress : IpAddress -> Task<MacInfo>
    abstract member getNameInfoForIpAsyncTry: useDns: bool -> ipAddress : IpAddress -> Task<NameInfo>

type INetworkBroker =
    abstract member outputDeviceInfoLines: deviceInfoLines : string[] -> unit

type IHelpTextBroker =
    abstract member printHeader : unit -> unit
    abstract member printUsage : unit -> unit
    abstract member printErrorSection : errorList : seq<string> -> unit
    abstract member printArgsInfo : argLineInfoList : ArgLineInfo array -> unit

type IExceptionBroker =
    abstract member printSingle : exn : Exception -> unit
    abstract member printAggregate : aggregateException : AggregateException -> unit

type IMetricsBroker =
    abstract member outputMeasurementTry: elementName : string -> ms : int64 -> unit

type IMacBlacklistBroker =
    abstract member getMacBlacklistAsyncTry : unit -> Task<string array>

type IIpBlacklistBroker =
    abstract member getIpBlacklistAsyncTry : unit -> Task<string array>
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
type IIpService =
    abstract member scanNetworkAsync : scanMacs : bool -> scanNames : bool -> useDns : bool -> network : IpNetwork ->
                                       Task<DeviceInfo[]>
    abstract member outputDeviceInfos : activesOnly : bool -> separator : string -> showMacs : bool ->
                                        showNames : bool -> deviceInfos : DeviceInfo array -> unit

type IHelpTextService =
    abstract member showHelp : appErrors : AppErrors -> int

type IExceptionService =
    abstract member outputException : exn : Exception -> unit

type IMetricsService =
    abstract member outputScanNetworkTimeTry : stopwatch : Stopwatch -> unit

type IOptionValidationService =
    abstract member ifErrorsShowAndExit: parserResult : ParserResult<ArgumentOptions> -> unit
//----------------------------------------------------------------------------------------------------------------------
