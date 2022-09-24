namespace Model

open CommandLine

type ArgumentOptions = {
    [<Option ('w', "timeout", Default = 500)>]
    TimeOut : int

    [<Option ('r', "retries", Default = 3)>]
    Retries : int

    [<Option ('s', "separador", Default = @"\t")>]
    Separator : string

    [<Option ('a', "activos", Default = false)>]
    ActiveOnly : bool

    [<Option ('m', "mac", Default = false)>]
    ShowMac : bool

    [<Value (0, MetaName="red", Required = true)>]
    Network : string
}
