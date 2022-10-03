namespace Model

type IpStatusArray =
    private IpStatusArray of IpStatus[]

        member this.value = let (IpStatusArray value) = this in value

        static member OfArray (value : IpStatus[])=
            value
            |> IpStatusArray
