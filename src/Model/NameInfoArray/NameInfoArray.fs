namespace Model

type NameInfoArray =
    private NameInfoArray of NameInfo[]

        static member private mapIpInfosToNameInfos ipInfos =

            ipInfos
            |> Array.map (fun x -> match x with
                                   | IpInfo.NameInfo nameInfo -> nameInfo
                                   | _ -> failwith $"Sólo se admiten datos de tipo {nameof NameInfo}")

        member this.value = let (NameInfoArray value) = this in value

        static member OfIpInfoArray value =
            value
            |> NameInfoArray.mapIpInfosToNameInfos
            |> NameInfoArray
