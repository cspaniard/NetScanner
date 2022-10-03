namespace Model

open CommandLine

type ArgumentOptions = {
    [<Option ('w', "ping-timeout", Default = 500)>]
    PingTimeOut : int

    [<Option ('r', "reintentos", Default = 3)>]
    Retries : int

    [<Option ('s', "separador", Default = @"\t")>]
    Separator : string

    [<Option ('a', "activos", Default = false)>]
    ActiveOnly : bool

    [<Option ('m', "mac", Default = false)>]
    ShowMac : bool

    [<Option ('n', "nombres", Default = false)>]
    ShowNames : bool

    [<Option ('l', "nombres-timeout", Default = 500)>]
    NameLookUpTimeOut : int

    [<Value (0, MetaName="red", Required = true)>]
    Network : string
}
