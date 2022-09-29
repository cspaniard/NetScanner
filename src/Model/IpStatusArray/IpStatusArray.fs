namespace Model

type IpStatusArray =
    private IpStatusArray of IpStatus[]

        static member private mapIpInfosToIpStatuses ipInfos =

            ipInfos
            |> Array.map (fun x -> match x with
                                   | IpInfo.IpStatus ipStatus -> ipStatus
                                   | _ -> failwith $"Sólo se admiten datos de tipo {nameof IpStatus}")

        member this.value = let (IpStatusArray value) = this in value

        static member OfIpInfoArray value =
            value
            |> IpStatusArray.mapIpInfosToIpStatuses
            |> IpStatusArray
