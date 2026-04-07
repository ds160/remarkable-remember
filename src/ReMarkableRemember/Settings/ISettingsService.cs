using System;
using ReMarkableRemember.Enumerations;
using ReMarkableRemember.Settings.Configuration;

namespace ReMarkableRemember.Settings;

public interface ISettingsService
{
    ISettingsConfiguration Configuration { get; }

    ApplicationThemes GetApplicationTheme()
    {
        return Enum.Parse<ApplicationThemes>(this.Configuration.ApplicationTheme);
    }

    String GetDateTimeFormat()
    {
        return "yyyy-MM-dd HH:mm";
    }
}
