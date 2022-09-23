namespace Model.Options

open CommandLine

type ArgumentOptions = {
    [<Option ('w', "timeout", Default = 500, HelpText = "Tiempo en ms de espera en cada ping.")>]
    TimeOut : int

    [<Option ('r', "retries", Default = 3, HelpText = "Número pings hasta dar la IP por inactiva.")>]
    Retries : int

    [<Option ('s', "separador", Default = @"\t", HelpText = "Separador entre campos.")>]
    Separator : string

    [<Option ('a', "activos", Default = false, HelpText = "Sólo devuelve las IPs activas.")>]
    ActiveOnly : bool

    [<Option ('m', "mac", Default = false, HelpText = "Muestra la MAC de cada IP activa.")>]
    ShowMac : bool

    [<Value (0, MetaName="red", Required = true, HelpText = "La red a escanear.")>]
    Network : string
}
