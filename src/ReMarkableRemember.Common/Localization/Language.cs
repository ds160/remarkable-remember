using System;
using System.Collections.Generic;
using ReMarkableRemember.Common.Localization.LocalStrings;

namespace ReMarkableRemember.Common.Localization;

public static class Language
{
    private static readonly ILocalStrings defaultLanguage;
    private static readonly Dictionary<String, ILocalStrings> supportedlanguages;

    static Language()
    {
        defaultLanguage = new Default();
        supportedlanguages = new Dictionary<String, ILocalStrings>()
        {
            { "en", new English() }
        };

        Current = defaultLanguage;
    }

    public static ILocalStrings Current { get; private set; }

    internal static IEnumerable<String> SupportedCodes { get { return supportedlanguages.Keys; } }

    internal static String Switch(String code)
    {
        if (supportedlanguages.TryGetValue(code, out ILocalStrings? language))
        {
            Current = language;
            return code;
        }
        else
        {
            Current = defaultLanguage;
            return String.Empty;
        }
    }
}
