using System;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Services.DataService.Helper;

namespace ReMarkableRemember.Services.DataService.Tests;

[TestFixture]
public sealed class DateTimeExtensionsTests
{
    [Test]
    public void FromDateTime_LocalTime_ConvertsToUtcIsoString()
    {
        DateTime local = new DateTime(2026, 5, 18, 14, 30, 0, DateTimeKind.Local);

        String result = local.FromDateTime();

        DateTime parsed = result.ToDateTime();
        parsed.Should().Be(local.ToUniversalTime());
        parsed.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Test]
    public void FromDateTime_UtcTime_RoundtripsWithoutLoss()
    {
        DateTime utc = new DateTime(2026, 5, 18, 14, 30, 45, 123, DateTimeKind.Utc);

        DateTime roundtripped = utc.FromDateTime().ToDateTime();

        roundtripped.Should().Be(utc);
    }

    [Test]
    public void ToDateTime_AlwaysReturnsUtc()
    {
        DateTime original = new DateTime(2026, 5, 18, 14, 30, 0, DateTimeKind.Utc);

        DateTime parsed = original.FromDateTime().ToDateTime();

        parsed.Kind.Should().Be(DateTimeKind.Utc);
    }
}
