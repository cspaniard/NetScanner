namespace NetScanner.Model

type IpInfo = IpInfo of IpAddress * active : bool
type IpInfoMac = IpInfoMac of IpAddress * active : bool * mac : string
type MacInfo = MacInfo of IpAddress * mac : string
