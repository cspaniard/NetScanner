namespace NetScanner.Model

type IpInfo = IpInfo of IpAddress * bool
type IpInfoMac = IpInfoMac of IpAddress * bool * string
type MacInfo = MacInfo of IpAddress * string
