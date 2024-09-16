namespace ReMarkableRemember.Models;

public enum TabletConnectionError
{
    Unknown,
    SshNotConfigured,
    SshNotConnected,
    UsbNotConnected,
    UsbNotActived,
    NotSupported,
}
