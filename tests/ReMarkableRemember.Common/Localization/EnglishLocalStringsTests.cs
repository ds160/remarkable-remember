using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Common.Localization.LocalStrings;

namespace ReMarkableRemember.Common.Localization.Tests;

[TestFixture]
public sealed class EnglishLocalStringsTests
{
    [Test]
    public void Default_InheritsFromEnglish_SoStringsMatch()
    {
        Default defaultStrings = new Default();
        English englishStrings = new English();

        defaultStrings.AboutTitle.Should().Be(englishStrings.AboutTitle);
        defaultStrings.ErrorTitle.Should().Be(englishStrings.ErrorTitle);
    }

    [Test]
    public void English_MyScriptLanguageNotSupported_InterpolatesLanguage()
    {
        English english = new English();

        String result = english.MyScriptLanguageNotSupported("xx_YY");

        result.Should().Contain("xx_YY");
    }

    [Test]
    public void English_MyScriptPageAnalyzeError_InterpolatesPageNumber()
    {
        English english = new English();

        String result = english.MyScriptPageAnalyzeError(42);

        result.Should().Contain("42");
    }

    [Test]
    public void English_NotebookBlockHeaderInvalid_InterpolatesByte()
    {
        English english = new English();

        String result = english.NotebookBlockHeaderInvalid(0xAB);

        result.Should().Contain(((Byte)0xAB).ToString(CultureInfo.InvariantCulture));
    }

    [Test]
    public void English_TabletFileFormatVersionInvalid_InterpolatesVersion()
    {
        English english = new English();

        String result = english.TabletFileFormatVersionInvalid(99);

        result.Should().Contain("99");
    }

    [Test]
    public void English_AllStringProperties_AreNonEmpty()
    {
        English strings = new English();

        // Spot check across the API; if a property accidentally returns "", a hard-to-trace
        // empty label appears in the UI. We sample the common categories.
        strings.AboutTitle.Should().NotBeNullOrWhiteSpace();
        strings.ButtonCancel.Should().NotBeNullOrWhiteSpace();
        strings.ButtonOK.Should().NotBeNullOrWhiteSpace();
        strings.ErrorTitle.Should().NotBeNullOrWhiteSpace();
        strings.JobBackup.Should().NotBeNullOrWhiteSpace();
        strings.JobSync.Should().NotBeNullOrWhiteSpace();
    }
}
