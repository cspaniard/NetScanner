namespace NetScanner.Options

open CommandLine

type ArgumentOptions = {
    [<Option ('w', "timeout", Default = 500, HelpText="Tiempo en ms de espera en cada ping.")>]
    TimeOut : int

    [<Option ('r', "retries", Default = 3, HelpText="Número pings hasta dar la IP por inactiva.")>]
    Retries : int

    [<Option ('s', "separador", Default = @"\t", HelpText="Separador entre campos.")>]
    Separator : string

    [<Value (0, MetaName="network", Required = true, HelpText="La red que escanear.")>]
    Network : string
}
