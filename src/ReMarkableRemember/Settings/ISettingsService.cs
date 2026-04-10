using System;
using ReMarkableRemember.Settings.Configuration;
using ReMarkableRemember.Settings.Enumerations;

namespace ReMarkableRemember.Settings;

public interface ISettingsService
{
    ISettingsConfiguration Configuration { get; }

    String GetDateTimeFormat()
    {
        return Enum.Parse<DateTimeFormats>(this.Configuration.DateTimeFormat) switch
        {
            DateTimeFormats.Hours24 => "yyyy-MM-dd HH:mm",
            DateTimeFormats.Hours12 => "yyyy-MM-dd hh:mm tt",
            _ => throw new NotImplementedException(),
        };
    }
}
