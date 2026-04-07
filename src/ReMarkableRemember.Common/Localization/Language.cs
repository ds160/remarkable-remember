using System;

namespace ReMarkableRemember.Common.Localization;

public static class Language
{
    static Language()
    {
        Provider = new LanguageProvider();
    }

    public static ILocalStrings Current { get { return Provider.Current; } }

    internal static ILanguageProvider Provider { get; private set; }

    public static void SetProvioder(ILanguageProvider provider)
    {
        String code = Provider.CurrentCode;

        Provider = provider;
        Provider.Switch(code);
    }
}
