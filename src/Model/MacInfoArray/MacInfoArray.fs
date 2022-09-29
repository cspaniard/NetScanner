namespace Model

type MacInfoArray =
    private MacInfoArray of MacInfo[]

        member this.value = let (MacInfoArray value) = this in value

        static member OfArray (value : MacInfo[])=
            value
            |> MacInfoArray
