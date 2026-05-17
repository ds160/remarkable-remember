using System;
using System.Collections.Generic;
using ReMarkableRemember.Common.Localization.Interfaces;
using ReMarkableRemember.Common.Localization.LocalStrings;

namespace ReMarkableRemember.Common.Localization;

internal sealed class LanguageProvider : ILanguageProvider
{
    private readonly ILocalStrings defaultLanguage;
    private readonly Dictionary<String, ILocalStrings> supportedlanguages;

    public LanguageProvider()
    {
        this.defaultLanguage = new Default();
        this.supportedlanguages = new Dictionary<String, ILocalStrings>()
        {
            { "en", new English() }
        };

        this.Current = this.defaultLanguage;
        this.CurrentCode = String.Empty;
    }

    public ILocalStrings Current { get; private set; }

    public String CurrentCode { get; private set; }

    public IEnumerable<String> SupportedCodes { get { return this.supportedlanguages.Keys; } }

    public void Switch(String code)
    {
        if (this.supportedlanguages.TryGetValue(code, out ILocalStrings? language))
        {
            this.Current = language;
            this.CurrentCode = code;
        }
        else
        {
            this.Current = this.defaultLanguage;
            this.CurrentCode = String.Empty;
        }
    }
}
