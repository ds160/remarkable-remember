using System;
using ReMarkableRemember.Common.Localization.LocalStrings;

namespace ReMarkableRemember.Common.Localization;

public static class Language
{
    static Language()
    {
        Current = new Default();
    }

    public static ILocalStrings Current { get; private set; }

    public static void Switch(String cultureCode)
    {
        Current = cultureCode switch
        {
            "en" => new English(),
            _ => new Default(),
        };
    }
}
