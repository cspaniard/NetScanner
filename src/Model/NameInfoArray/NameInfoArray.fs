namespace Model

type NameInfoArray =
    private NameInfoArray of NameInfo[]

        member this.value = let (NameInfoArray value) = this in value

        static member OfArray (value : NameInfo[])=
            value
            |> NameInfoArray
