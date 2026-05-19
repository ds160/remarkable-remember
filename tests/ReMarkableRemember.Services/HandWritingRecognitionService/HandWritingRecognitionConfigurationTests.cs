using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Services.ConfigurationService.Configuration;
using ReMarkableRemember.Services.HandWritingRecognitionService.Configuration;

namespace ReMarkableRemember.Services.HandWritingRecognitionService.Tests;

[TestFixture]
public sealed class HandWritingRecognitionConfigurationTests
{
    [Test]
    public void Defaults_ApplicationKeyAndHmacAreEmptyAndLanguageIsEnUs()
    {
        HandWritingRecognitionConfigurationMyScript config = new HandWritingRecognitionConfigurationMyScript();

        config.ApplicationKey.Should().BeEmpty();
        config.HmacKey.Should().BeEmpty();
        config.Language.Should().Be("en_US");
    }

    [Test]
    public void Prefix_IsMyScript()
    {
        HandWritingRecognitionConfigurationMyScript config = new HandWritingRecognitionConfigurationMyScript();

        ((IConfiguration)config).GetPrefix().Should().Be("MyScript");
    }
}
