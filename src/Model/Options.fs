namespace Model

open CommandLine

type ArgumentOptions = {
    [<Option ('w', "ping-timeout", Default = 500)>]
    PingTimeOut : int

    [<Option ('r', "retries", Default = 3)>]
    Retries : int

    [<Option ('s', "separador", Default = @"\t")>]
    Separator : string

    [<Option ('a', "activos", Default = false)>]
    ActiveOnly : bool

    [<Option ('m', "mac", Default = false)>]
    ShowMac : bool

    [<Option ('l', "name-timeout", Default = 500)>]
    NameLookUpTimeOut : int

    [<Value (0, MetaName="red", Required = true)>]
    Network : string
}
