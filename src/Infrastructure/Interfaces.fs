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
    abstract member outputDeviceInfoLinesTry: deviceInfoLines : string[] -> unit

type IHelpTextBroker =
    abstract member printHeaderTry: unit -> unit
    abstract member printUsageTry: unit -> unit
    abstract member printErrorSectionTry: errorList : seq<string> -> unit
    abstract member printArgsInfoTry: argLineInfoList : ArgLineInfo array -> unit

type IExceptionBroker =
    abstract member printSingleTry: exn : Exception -> unit
    abstract member printAggregateTry: aggregateException : AggregateException -> unit

type IMetricsBroker =
    abstract member outputMeasurementTry: elementName : string -> ms : int64 -> unit

type IMacBlacklistBroker =
    abstract member getMacBlacklistAsyncTry: unit -> Task<string seq>

type IIpBlacklistBroker =
    abstract member getIpBlacklistAsyncTry: unit -> Task<string seq>
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
type IIpService =
    abstract member scanNetworkAsyncTry: scanMacs : bool -> scanNames : bool -> useDns : bool ->
                                         network : IpNetwork -> Task<DeviceInfo[]>
    abstract member outputDeviceInfosTry: activesOnly : bool -> separator : string -> showMacs : bool ->
                                          showNames : bool -> deviceInfos : DeviceInfo array -> unit

type IHelpTextService =
    abstract member showHelpTry: appErrors : AppErrors -> int

type IExceptionService =
    abstract member outputExceptionTry: exn : Exception -> unit

type IMetricsService =
    abstract member outputScanNetworkTimeTry: stopwatch : Stopwatch -> unit

type IOptionValidationService =
    abstract member ifErrorsShowAndExit: parserResult : ParserResult<ArgumentOptions> -> unit
//----------------------------------------------------------------------------------------------------------------------
