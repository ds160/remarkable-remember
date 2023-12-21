using System;
using System.Globalization;

namespace ReMarkableRemember.Helper;

public static class DateTimeExtensions
{
    public static String FromDateTime(this DateTime value)
    {
        return value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
    }

    public static DateTime ToDateTime(this String value)
    {
        return DateTime.Parse(value, CultureInfo.InvariantCulture).ToUniversalTime();
    }

    public static String ToDisplayString(this DateTime value)
    {
        return value.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    }
}
