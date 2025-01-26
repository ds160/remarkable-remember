using System;
using System.Globalization;

namespace ReMarkableRemember.Services.DataService.Helper;

internal static class DateTimeExtensions
{
    public static String FromDateTime(this DateTime value)
    {
        return value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
    }

    public static DateTime ToDateTime(this String value)
    {
        return DateTime.Parse(value, CultureInfo.InvariantCulture).ToUniversalTime();
    }
}
