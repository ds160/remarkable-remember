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

    public static IEnumerable<String> SupportedCultureCodes { get { return supportedlanguages.Keys; } }

    internal static void Switch(String cultureCode)
    {
        supportedlanguages.TryGetValue(cultureCode, out ILocalStrings? language);
        Current = language ?? defaultLanguage;
    }
}
