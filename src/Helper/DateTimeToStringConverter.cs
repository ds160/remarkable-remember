using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ReMarkableRemember.Helper;

public class DateTimeToStringConverter : ValueConverter<DateTime, String>
{
    public DateTimeToStringConverter() : base(value => value.FromDateTime(), value => value.ToDateTime(), null)
    {
    }
}
