using System;
using System.Globalization;

namespace ReMarkableRemember.Helper;

public static class DateTimeExtensions
{
    public static String ToDisplayString(this DateTime value, String format)
    {
        return value.ToLocalTime().ToString(format, CultureInfo.InvariantCulture);
    }
}
