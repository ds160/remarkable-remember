using System;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Settings.Configuration;

public sealed class SettingsConfiguration : ConfigurationBase, ISettingsConfiguration
{
    public SettingsConfiguration() : base("ApplicationSettings")
    {
        this.ApplicationLanguage = String.Empty;
    }

    public String ApplicationLanguage
    {
        get;
        set { field = Language.Switch(value); }
    }
}
