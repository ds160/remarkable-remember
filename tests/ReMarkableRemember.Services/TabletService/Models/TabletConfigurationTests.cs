using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Services.ConfigurationService.Configuration;
using ReMarkableRemember.Services.TabletService.Configuration;

namespace ReMarkableRemember.Services.TabletService.Tests.Models;

[TestFixture]
public sealed class TabletConfigurationTests
{
    [Test]
    public void Defaults_AllStringPropertiesAreEmpty()
    {
        TabletConfiguration config = new TabletConfiguration();

        config.Backup.Should().BeEmpty();
        config.IP.Should().BeEmpty();
        config.Password.Should().BeEmpty();
    }

    [Test]
    public void Prefix_IsTablet()
    {
        TabletConfiguration config = new TabletConfiguration();

        ((IConfiguration)config).GetPrefix().Should().Be("Tablet");
    }
}
