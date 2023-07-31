namespace Model

open CommandLine
open Model.Constants

type ArgumentOptions = {
    [<Option ('w', "ping-timeout", Default = DEF_PING_TIMEOUT)>]
    PingTimeOut : int

    [<Option ('r', "reintentos", Default = DEF_RETRIES)>]
    Retries : int

    [<Option ('s', "separador", Default = @"\t")>]
    Separator : string

    [<Option ('a', "activos", Default = false)>]
    ActivesOnly : bool

    [<Option ('m', "mac", Default = false)>]
    ShowMacs : bool

    [<Option ('n', "nombres", Default = false)>]
    ShowNames : bool

    [<Option ('l', "nombres-timeout", Default = DEF_NAME_LOOKUP_TIMEOUT)>]
    NameLookUpTimeOut : int

    [<Option ('d', "debug", Default = false)>]
    Debug : bool

    [<Option ('b', "blacklist", Default = "")>]
    MacBlackListFileName : string

    [<Option ('B',"ip-blacklist", Default = "")>]
    IpBlackListFileName : string

    [<Value (0, MetaName="red", Required = true)>]
    Network : string
}
