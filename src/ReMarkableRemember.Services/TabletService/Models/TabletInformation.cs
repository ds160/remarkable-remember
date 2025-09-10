using System;

namespace ReMarkableRemember.Services.TabletService.Models;

public sealed class TabletInformation
{
    private static readonly Version WEB_INTERFACE_HACK_MIN_VERSION = new Version("2.15.0.0");
    private static readonly Version WEB_INTERFACE_SUPPORT_MAX_VERSION = new Version("3.16.0.0");

    public TabletInformation(TabletType type, Version softwareVersion)
    {
        this.LamyEraserSupport = type is TabletType.rM1 or TabletType.rM2;
        this.Resolution = type switch
        {
            TabletType.rM1 => 226,
            TabletType.rM2 => 226,
            TabletType.rMPaperPro => 229,
            TabletType.rMPaperProMove => 264,
            _ => throw new NotImplementedException(),
        };
        this.SoftwareVersion = softwareVersion;
        this.Type = type;
        this.WebInterfaceOnBootHack = softwareVersion.CompareTo(WEB_INTERFACE_HACK_MIN_VERSION) >= 0;
        this.WebInterfaceOnBootSupport = softwareVersion.CompareTo(WEB_INTERFACE_SUPPORT_MAX_VERSION) < 0;
    }

    public Boolean LamyEraserSupport { get; }

    internal Int32 Resolution { get; }

    public Version SoftwareVersion { get; }

    public TabletType Type { get; }

    internal Boolean WebInterfaceOnBootHack { get; }

    public Boolean WebInterfaceOnBootSupport { get; }
}
