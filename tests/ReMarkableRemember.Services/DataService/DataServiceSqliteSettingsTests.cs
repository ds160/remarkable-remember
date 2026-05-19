using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Services.DataService.Models;
using ReMarkableRemember.Services.DataService.Tests.Fixtures;

namespace ReMarkableRemember.Services.DataService.Tests;

[TestFixture]
public sealed class DataServiceSqliteSettingsTests
{
    private InMemoryDataServiceFixture fixture = null!;
    private DataServiceSqlite service = null!;

    [SetUp]
    public void SetUp()
    {
        this.fixture = new InMemoryDataServiceFixture();
        this.service = this.fixture.Service;
    }

    [TearDown]
    public void TearDown()
    {
        this.fixture.Dispose();
    }

    [Test]
    public async Task SaveSettings_ThenLoad_RoundtripsValues()
    {
        SettingData[] toSave = new[]
        {
            new SettingData("Prefix", "Key1", "value1"),
            new SettingData("Prefix", "Key2", "value2"),
        };

        await this.service.SaveSettings(toSave);

        SettingData[] toLoad = new[]
        {
            new SettingData("Prefix", "Key1", "initial"),
            new SettingData("Prefix", "Key2", "initial"),
        };
        await this.service.LoadSettings(toLoad);

        toLoad[0].Value.Should().Be("value1");
        toLoad[1].Value.Should().Be("value2");
    }

    [Test]
    public async Task SaveSettings_UpdatesExistingValue()
    {
        await this.service.SaveSettings(new[] { new SettingData("Prefix", "Key1", "v1") });
        await this.service.SaveSettings(new[] { new SettingData("Prefix", "Key1", "v2") });

        SettingData fetch = new SettingData("Prefix", "Key1", "default");
        await this.service.LoadSettings(new[] { fetch });

        fetch.Value.Should().Be("v2");
    }

    [Test]
    public async Task LoadSettings_UnknownKey_LeavesValueUntouched()
    {
        SettingData unknown = new SettingData("Other", "Missing", "untouched");

        await this.service.LoadSettings(new[] { unknown });

        unknown.Value.Should().Be("untouched");
    }

    [Test]
    public async Task SaveSettings_DifferentPrefixesAreIsolated()
    {
        await this.service.SaveSettings(new[]
        {
            new SettingData("PrefixA", "Key", "from-A"),
            new SettingData("PrefixB", "Key", "from-B"),
        });

        SettingData fetchA = new SettingData("PrefixA", "Key", "x");
        SettingData fetchB = new SettingData("PrefixB", "Key", "x");
        await this.service.LoadSettings(new[] { fetchA, fetchB });

        fetchA.Value.Should().Be("from-A");
        fetchB.Value.Should().Be("from-B");
    }
}
