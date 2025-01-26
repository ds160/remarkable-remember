using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ReMarkableRemember.Services.DataService.Helper;

internal sealed class DateTimeToStringConverter : ValueConverter<DateTime, String>
{
    public DateTimeToStringConverter() : base(value => value.FromDateTime(), value => value.ToDateTime(), null)
    {
    }
}
