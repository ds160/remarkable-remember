using System;

namespace ReMarkableRemember.Services.TabletService.Models;

public enum TabletType
{
    rM1,
    rM2,
    rMPaperPro,
    rMPaperProMove
}

public static class TabletTypeExtensions
{
    public static String GetDisplayText(this TabletType type)
    {
        return type switch
        {
            TabletType.rM1 => "reMarkable 1",
            TabletType.rM2 => "reMarkable 2",
            TabletType.rMPaperPro => "reMarkable Paper Pro",
            TabletType.rMPaperProMove => "reMarkable Paper Pro Move",
            _ => throw new NotImplementedException(),
        };
    }
}
