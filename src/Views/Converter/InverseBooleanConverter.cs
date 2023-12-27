using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace ReMarkableRemember.Views.Converter;

public sealed class InverseBooleanConverter : IValueConverter
{
    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)
    {
        if (value is Boolean valueBoolean && targetType == typeof(Boolean))
        {
            return !valueBoolean;
        }

        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public Object ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
