using System;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Services.ConfigurationService.Configuration;
using ReMarkableRemember.Settings.Enumerations;

namespace ReMarkableRemember.Settings.Configuration;

public sealed class SettingsConfiguration : ConfigurationBase, ISettingsConfiguration
{
    public SettingsConfiguration() : base("Settings")
    {
        this.ApplicationTheme = Default<ApplicationThemes>();
        this.DateTimeFormat = Default<DateTimeFormats>();
    }

    public String ApplicationLanguage
    {
        get { return Language.Provider.CurrentCode; }
        set { Language.Provider.Switch(value); }
    }

    public String ApplicationTheme
    {
        get;
        set { field = Verify<ApplicationThemes>(value); }
    }

    public String DateTimeFormat
    {
        get;
        set { field = Verify<DateTimeFormats>(value); }
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
