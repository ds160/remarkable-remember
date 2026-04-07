using System;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Enumerations;
using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Settings.Configuration;

public sealed class SettingsConfiguration : ConfigurationBase, ISettingsConfiguration
{
    public SettingsConfiguration() : base("Settings")
    {
        this.ApplicationLanguage = String.Empty;
        this.ApplicationTheme = Default<ApplicationThemes>();
    }

    public String ApplicationLanguage
    {
        get;
        set { field = Language.Switch(value); }
    }

    public String ApplicationTheme
    {
        get;
        set { field = Verify<ApplicationThemes>(value); }
    }

    private static String Default<T>() where T : struct, Enum
    {
        return Enum.GetName<T>(default) ?? throw new NotImplementedException();
    }

    private static String Verify<T>(String value) where T : struct, Enum
    {
        return Enum.TryParse<T>(value, out _) ? value : Default<T>();
    }
}
