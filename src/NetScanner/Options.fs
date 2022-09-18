namespace NetScanner.Options

open CommandLine

type ArgumentOptions = {
    [<Option ('w', "timeout", Default = 500, HelpText = "Tiempo en ms de espera en cada ping.")>]
    TimeOut : int

    [<Option ('r', "retries", Default = 3, HelpText = "Número pings hasta dar la IP por inactiva.")>]
    Retries : int

    [<Option ('s', "separador", Default = @"\t", HelpText = "Separador entre campos.")>]
    Separator : string

    [<Option ('a', "activos", Default = false, HelpText = "Sólo devuelve los activos.")>]
    ActiveOnly : bool

    [<Value (0, MetaName="network", Required = true, HelpText = "La red que escanear.")>]
    Network : string
}

module NetScanner =

    let showErrors (err : NotParsed<ArgumentOptions>) =
        let properties = err.TypeInfo.Current.GetProperties()

        for property in properties do
            let customAttributes = property.GetCustomAttributes(true)

            match customAttributes[0] with
            | :? OptionAttribute as option ->
                printfn $"-{option.ShortName}  --{option.LongName}\t{option.Default}\t{option.HelpText}"
            | :? ValueAttribute as value ->
                printfn $"{value.MetaName} (Idx: {value.Index})\t{value.Default}\t{value.HelpText}"
            | :? VerbAttribute as verb ->
                printfn $"{verb.Name} (Idx: {verb.IsDefault})\t{verb.HelpText}"
            | _ -> printfn "Atributo no identificado."

            printfn ""
