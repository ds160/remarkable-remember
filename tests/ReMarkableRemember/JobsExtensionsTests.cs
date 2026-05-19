using System;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.ViewModels.Enumerations;

namespace ReMarkableRemember.Tests;

[TestFixture]
public sealed class JobsExtensionsTests
{
    [Test]
    public void GetDisplayText_None_ReturnsNull()
    {
        Jobs.None.GetDisplayText().Should().BeNull();
    }

    [Test]
    public void GetDisplayText_SingleJob_ReturnsNonEmpty()
    {
        Jobs.Backup.GetDisplayText().Should().NotBeNullOrEmpty();
    }

    [Test]
    public void GetDisplayText_CombinedJobs_IncludesBothLabelsAndJoiner()
    {
        String? text = (Jobs.Backup | Jobs.Sync).GetDisplayText();

        text.Should().NotBeNull();
        // joiner is " and " (English default) - test that both single-job labels appear
        text.Should().Contain(Jobs.Backup.GetDisplayText()!);
        text.Should().Contain(Jobs.Sync.GetDisplayText()!);
    }
}
