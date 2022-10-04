namespace Model

type DeviceInfoArray =
    private DeviceInfoArray of DeviceInfo[]

        member this.value = let (DeviceInfoArray value) = this in value

        static member OfArray (value : DeviceInfo[])=
            value
            |> DeviceInfoArray
