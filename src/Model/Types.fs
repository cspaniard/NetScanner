namespace NetScanner.Model

type IpInfo =
    IpInfo of IpAddress * active : bool
        static member tuppleOf (ipInfo : IpInfo) = let (IpInfo (address, active)) = ipInfo in (address, active)

type IpInfoMac = IpInfoMac of IpAddress * active : bool * mac : string
type MacInfo = MacInfo of IpAddress * mac : string
