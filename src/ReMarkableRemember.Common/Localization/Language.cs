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

    public static void Switch(CultureInfo cultureInfo)
    {
        Current = cultureInfo.TwoLetterISOLanguageName switch
        {
            "en" => new English(),
            _ => new Default(),
        };

        CurrentChanged?.Invoke(null, EventArgs.Empty);
    }

    public static event EventHandler? CurrentChanged;
}
