using System;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Common.Localization.Interfaces;
using ReMarkableRemember.Services.ConfigurationService.Configuration;
using ReMarkableRemember.Settings.Configuration;
using ReMarkableRemember.Settings.Enumerations;

namespace ReMarkableRemember.Tests;

[TestFixture]
public sealed class SettingsConfigurationTests
{
    private ILanguageProvider originalProvider = null!;

    [SetUp]
    public void SetUp()
    {
        this.originalProvider = Language.Provider;
        Language.SetProvioder(new TestLanguageProvider());
    }

    [TearDown]
    public void TearDown()
    {
        Language.SetProvioder(this.originalProvider);
    }

    [Test]
    public void Defaults_AreEnumDefaultNames()
    {
        SettingsConfiguration config = new SettingsConfiguration();

        config.ApplicationTheme.Should().Be(ApplicationThemes.Default.ToString());
        config.DateTimeFormat.Should().Be(DateTimeFormats.Hours24.ToString());
    }

    [Test]
    public void Prefix_IsSettings()
    {
        SettingsConfiguration config = new SettingsConfiguration();

        ((IConfiguration)config).GetPrefix().Should().Be("Settings");
    }

    [Test]
    public void ApplicationTheme_AcceptsKnownEnumValue()
    {
        SettingsConfiguration config = new SettingsConfiguration
        {
            ApplicationTheme = ApplicationThemes.Dark.ToString()
        };

        config.ApplicationTheme.Should().Be(ApplicationThemes.Dark.ToString());
    }

    [Test]
    public void ApplicationTheme_FallsBackToDefaultOnInvalidValue()
    {
        SettingsConfiguration config = new SettingsConfiguration
        {
            ApplicationTheme = ApplicationThemes.Light.ToString()
        };

        config.ApplicationTheme = "not-a-theme";

        config.ApplicationTheme.Should().Be(ApplicationThemes.Default.ToString());
    }

    [Test]
    public void DateTimeFormat_AcceptsKnownEnumValue()
    {
        SettingsConfiguration config = new SettingsConfiguration
        {
            DateTimeFormat = DateTimeFormats.Hours12.ToString()
        };

        config.DateTimeFormat.Should().Be(DateTimeFormats.Hours12.ToString());
    }

    [Test]
    public void DateTimeFormat_FallsBackToDefaultOnInvalidValue()
    {
        SettingsConfiguration config = new SettingsConfiguration
        {
            DateTimeFormat = "bogus"
        };

        config.DateTimeFormat.Should().Be(DateTimeFormats.Hours24.ToString());
    }

    [Test]
    public void ApplicationLanguage_GetterAndSetterRouteThroughProvider()
    {
        SettingsConfiguration config = new SettingsConfiguration();
        TestLanguageProvider provider = (TestLanguageProvider)Language.Provider;
        provider.CurrentCode = "en";

        String value = config.ApplicationLanguage;

        value.Should().Be("en");

        config.ApplicationLanguage = "de";
        provider.LastSwitchedCode.Should().Be("de");
    }

    private sealed class TestLanguageProvider : ILanguageProvider
    {
        public ILocalStrings Current { get; } = Moq.Mock.Of<ILocalStrings>();
        public String CurrentCode { get; set; } = String.Empty;
        public String DefaultCode { get; } = String.Empty;
        public System.Collections.Generic.IEnumerable<String> SupportedCodes { get; } = new[] { "en", "de" };
        public String? LastSwitchedCode { get; private set; }

        public void Switch(String code)
        {
            this.LastSwitchedCode = code;
            this.CurrentCode = code;
        }
    }
}
