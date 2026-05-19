using System;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Services.DataService.Models;

namespace ReMarkableRemember.Services.DataService.Tests;

[TestFixture]
public sealed class SettingDataTests
{
    [Test]
    public void DatabaseKey_CombinesPrefixAndKeyWithSpace()
    {
        SettingData data = new SettingData("MyScript", "Language", "en_US");

        data.DatabaseKey.Should().Be("MyScript Language");
    }

    [Test]
    public void Constructor_StoresAllValues()
    {
        SettingData data = new SettingData("Pre", "K", "V");

        data.Prefix.Should().Be("Pre");
        data.Key.Should().Be("K");
        data.Value.Should().Be("V");
    }

    [Test]
    public void Value_IsMutable()
    {
        SettingData data = new SettingData("Pre", "K", "V")
        {
            Value = "new-value"
        };

        data.Value.Should().Be("new-value");
    }
}

[TestFixture]
public sealed class TemplateDataTests
{
    [Test]
    public void Constructor_StoresAllValues()
    {
        Byte[] png = new Byte[] { 1, 2, 3 };
        Byte[] svg = new Byte[] { 4, 5, 6 };

        TemplateData data = new TemplateData("Cat", "Name", "icon", png, svg);

        data.Category.Should().Be("Cat");
        data.Name.Should().Be("Name");
        data.IconCode.Should().Be("icon");
        data.BytesPng.Should().BeSameAs(png);
        data.BytesSvg.Should().BeSameAs(svg);
    }
}
