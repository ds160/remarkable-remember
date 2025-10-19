using System;

namespace ReMarkableRemember.Services.TabletService.Models;

public sealed class TabletInformation
{
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
    }

    public Boolean LamyEraserSupport { get; }

    internal Int32 Resolution { get; }

    public Version SoftwareVersion { get; }

    public TabletType Type { get; }
}
