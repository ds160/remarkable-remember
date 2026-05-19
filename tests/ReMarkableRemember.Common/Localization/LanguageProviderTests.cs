using System;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Common.Localization.Interfaces;

namespace ReMarkableRemember.Common.Localization.Tests;

[TestFixture]
public sealed class LanguageProviderTests
{
    [Test]
    public void Constructor_DefaultsToDefaultLanguageAndEmptyCode()
    {
        LanguageProvider provider = new LanguageProvider();

        provider.Current.Should().NotBeNull();
        provider.CurrentCode.Should().Be(provider.DefaultCode);
        provider.DefaultCode.Should().Be(String.Empty);
    }

    [Test]
    public void SupportedCodes_IncludesEnglish()
    {
        LanguageProvider provider = new LanguageProvider();

        provider.SupportedCodes.Should().Contain("en");
    }

    [Test]
    public void Switch_KnownCode_SetsCurrentCode()
    {
        LanguageProvider provider = new LanguageProvider();
        ILocalStrings before = provider.Current;

        provider.Switch("en");

        provider.CurrentCode.Should().Be("en");
        provider.Current.Should().NotBeNull();
        provider.Current.Should().NotBeSameAs(before, "switching from default to English should change the strings instance");
    }

    [TestCase("fr")]
    [TestCase("DE")]
    [TestCase("")]
    [TestCase("not-a-code")]
    public void Switch_UnknownCode_FallsBackToDefault(String code)
    {
        LanguageProvider provider = new LanguageProvider();
        provider.Switch("en");

        provider.Switch(code);

        provider.CurrentCode.Should().Be(provider.DefaultCode);
    }

    [Test]
    public void Switch_IsCaseSensitive()
    {
        LanguageProvider provider = new LanguageProvider();

        provider.Switch("EN");

        provider.CurrentCode.Should().Be(provider.DefaultCode, "lookup uses a case-sensitive dictionary");
    }
}
