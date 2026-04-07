using System;
using System.Globalization;
using ReMarkableRemember.Settings;

namespace ReMarkableRemember.Helper;

public static class DateTimeExtensions
{
    public static String ToDisplayString(this DateTime value, ISettingsService settingsService)
    {
        return value.ToLocalTime().ToString(settingsService.GetDateTimeFormat(), CultureInfo.InvariantCulture);
    }
}
