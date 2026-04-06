using System;
using System.Globalization;
using ReMarkableRemember.Common.Localization.LocalStrings;

namespace ReMarkableRemember.Common.Localization;

public static class Language
{
    static Language()
    {
        Current = new Default();
    }

    public static ILocalStrings Current { get; private set; }

    internal static void Switch(String cultureCode)
    {
        Current = CultureInfo.GetCultureInfo(cultureCode).TwoLetterISOLanguageName switch
        {
            "en" => new English(),
            _ => new Default(),
        };
    }
}
