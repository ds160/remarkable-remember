using System;
using System.Linq;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Services.LocalizationService.Configuration;

public sealed class LocalizationConfiguration : ConfigurationBase, ILocalizationConfiguration
{
    public LocalizationConfiguration() : base("Localization")
    {
        this.CultureCode = String.Empty;
    }

    public String CultureCode
    {
        get;
        set
        {
            String cultureCode = Language.SupportedCultureCodes.SingleOrDefault(code => code.Equals(value, StringComparison.Ordinal)) ?? String.Empty;
            if (!String.Equals(field, cultureCode, StringComparison.Ordinal))
            {
                field = cultureCode;
                Language.Switch(cultureCode);
            }
        }
    }
}
