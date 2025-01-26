using System;
using System.Globalization;

namespace ReMarkableRemember.Helper;

public static class DateTimeExtensions
{
    public static String ToDisplayString(this DateTime value)
    {
        return value.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    }
}
