using System;
using ReMarkableRemember.Settings.Configuration;

namespace ReMarkableRemember.Settings;

public interface ISettingsService
{
    ISettingsConfiguration Configuration { get; }

    String GetDateTimeFormat()
    {
        return "yyyy-MM-dd HH:mm";
    }
}
