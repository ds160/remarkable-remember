using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Services.HandWritingRecognitionService.MyScript;

namespace ReMarkableRemember.Services.HandWritingRecognitionService.Tests;

[TestFixture]
public sealed class MyScriptLanguagesTests
{
    [Test]
    public void Supported_ContainsCommonLanguages()
    {
        MyScriptLanguages.Supported.Should().Contain("en_US");
        MyScriptLanguages.Supported.Should().Contain("de_DE");
        MyScriptLanguages.Supported.Should().Contain("fr_FR");
    }

    [Test]
    public void Supported_HasEntries()
    {
        MyScriptLanguages.Supported.Should().NotBeEmpty();
    }
}
