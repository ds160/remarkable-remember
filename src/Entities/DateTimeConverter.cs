using System;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ReMarkableRemember.Entities;

public class DateTimeConverter : ValueConverter<DateTime, String>
{
    public DateTimeConverter() : base(Serialize, Deserialize, null)
    {
    }

    private static readonly Expression<Func<String, DateTime>> Deserialize = value => DateTime.Parse(value, CultureInfo.InvariantCulture).ToUniversalTime();

    private static readonly Expression<Func<DateTime, String>> Serialize = value => value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
}
