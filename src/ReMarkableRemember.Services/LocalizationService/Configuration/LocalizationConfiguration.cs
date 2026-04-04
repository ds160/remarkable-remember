using System;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Services.LocalizationService.Configuration;

public sealed class LocalizationConfiguration : ConfigurationBase, ILocalizationConfiguration
{
    public LocalizationConfiguration() : base("Localization")
    {
        this.CultureCode = String.Empty;
        this.DateTimeFormat = "yyyy-MM-dd HH:mm";
    }

    public String CultureCode
    {
        get;
        set
        {
            if (!String.Equals(field, value, StringComparison.Ordinal))
            {
                field = value;
                Language.Switch(value);
            }
        }
    }

    public String DateTimeFormat { get; set; }
}
