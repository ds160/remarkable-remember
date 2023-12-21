using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace ReMarkableRemember.Views.Converter;

public sealed class NullableBooleanConverter : IValueConverter
{
    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)
    {
        if (targetType != null && targetType.IsAssignableTo(typeof(Boolean)))
        {
            Boolean parameterValue;
            Boolean resultIfNull = !Boolean.TryParse(parameter as String, out parameterValue) || parameterValue;
            return (value == null) ? resultIfNull : !resultIfNull;
        }

        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public Object ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
