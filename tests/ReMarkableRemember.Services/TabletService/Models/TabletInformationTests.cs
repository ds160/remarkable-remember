using System;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Services.TabletService.Exceptions;
using ReMarkableRemember.Services.TabletService.Models;
using ReMarkableRemember.Services.TabletService.Models.Enumerations;

namespace ReMarkableRemember.Services.TabletService.Tests.Models;

[TestFixture]
public sealed class TabletInformationTests
{
    [TestCase(TabletType.rM1, true)]
    [TestCase(TabletType.rM2, true)]
    [TestCase(TabletType.rMPaperPro, false)]
    [TestCase(TabletType.rMPaperProMove, false)]
    public void LamyEraserSupport_OnlyTrueForRm1Or2(TabletType type, Boolean expected)
    {
        TabletInformation info = new TabletInformation(type, new Version(3, 0, 0));

        info.LamyEraserSupport.Should().Be(expected);
    }

    [TestCase(TabletType.rM1, 226)]
    [TestCase(TabletType.rM2, 226)]
    [TestCase(TabletType.rMPaperPro, 229)]
    [TestCase(TabletType.rMPaperProMove, 264)]
    public void Resolution_MapsByTabletType(TabletType type, Int32 expected)
    {
        TabletInformation info = new TabletInformation(type, new Version(3, 0, 0));

        info.Resolution.Should().Be(expected);
    }
}

[TestFixture]
public sealed class TabletConnectionStatusTests
{
    [Test]
    public void Default_HasUnknownErrorAndNoInformation()
    {
        TabletConnectionStatus.Default.Error.Should().Be(TabletError.Unknown);
        TabletConnectionStatus.Default.Information.Should().BeNull();
    }
}

[TestFixture]
public sealed class TabletTypeExtensionsTests
{
    [TestCase(TabletType.rM1, "reMarkable 1")]
    [TestCase(TabletType.rM2, "reMarkable 2")]
    [TestCase(TabletType.rMPaperPro, "reMarkable Paper Pro")]
    [TestCase(TabletType.rMPaperProMove, "reMarkable Paper Pro Move")]
    public void GetDisplayText_ReturnsHumanReadableName(TabletType type, String expected)
    {
        type.GetDisplayText().Should().Be(expected);
    }
}

[TestFixture]
public sealed class TabletExceptionTests
{
    [Test]
    public void Constructor_WithErrorAndMessage_PreservesBoth()
    {
        TabletException ex = new TabletException(TabletError.SshNotConnected, "boom");

        ex.Error.Should().Be(TabletError.SshNotConnected);
        ex.Message.Should().Be("boom");
    }

    [Test]
    public void Constructor_WithMessageOnly_ErrorIsDefault()
    {
        TabletException ex = new TabletException("oops");

        ex.Error.Should().Be(TabletError.Unknown);
        ex.Message.Should().Be("oops");
    }
}
