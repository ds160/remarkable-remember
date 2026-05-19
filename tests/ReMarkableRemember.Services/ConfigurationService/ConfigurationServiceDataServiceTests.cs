using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Services.ConfigurationService.Tests.Fakes;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.DataService.Models;

namespace ReMarkableRemember.Services.ConfigurationService.Tests;

[TestFixture]
public sealed class ConfigurationServiceDataServiceTests
{
    [Test]
    public async Task Save_EmitsOneSettingPerStringProperty()
    {
        IEnumerable<SettingData> captured = Enumerable.Empty<SettingData>();
        Mock<IDataService> dataMock = new Mock<IDataService>();
        dataMock.Setup(d => d.SaveSettings(It.IsAny<IEnumerable<SettingData>>()))
            .Callback<IEnumerable<SettingData>>(settings => captured = settings.ToArray())
            .Returns(Task.CompletedTask);

        ConfigurationServiceDataService service = new ConfigurationServiceDataService(dataMock.Object);
        TestConfiguration configuration = new TestConfiguration { StringValue = "hello", AnotherString = "world", IntegerValue = 7 };

        await service.Save(configuration);

        captured.Select(s => s.Key).Should().BeEquivalentTo(new[] { nameof(TestConfiguration.StringValue), nameof(TestConfiguration.AnotherString) });
        captured.All(s => s.Prefix == "TestPrefix").Should().BeTrue();
    }

    [Test]
    public async Task Save_NullStringProperty_CoalescedToEmpty()
    {
        IEnumerable<SettingData> captured = Enumerable.Empty<SettingData>();
        Mock<IDataService> dataMock = new Mock<IDataService>();
        dataMock.Setup(d => d.SaveSettings(It.IsAny<IEnumerable<SettingData>>()))
            .Callback<IEnumerable<SettingData>>(settings => captured = settings.ToArray())
            .Returns(Task.CompletedTask);

        ConfigurationServiceDataService service = new ConfigurationServiceDataService(dataMock.Object);
        TestConfiguration configuration = new TestConfiguration { StringValue = null! };

        await service.Save(configuration);

        captured.Single(s => s.Key == nameof(TestConfiguration.StringValue)).Value.Should().Be(String.Empty);
    }

    [Test]
    public async Task Load_WritesValuesBackFromDataService()
    {
        Mock<IDataService> dataMock = new Mock<IDataService>();
        dataMock.Setup(d => d.LoadSettings(It.IsAny<IEnumerable<SettingData>>()))
            .Callback<IEnumerable<SettingData>>(settings =>
            {
                foreach (SettingData setting in settings)
                {
                    if (setting.Key == nameof(TestConfiguration.StringValue)) { setting.Value = "loaded-value"; }
                    if (setting.Key == nameof(TestConfiguration.AnotherString)) { setting.Value = "loaded-other"; }
                }
            })
            .Returns(Task.CompletedTask);

        ConfigurationServiceDataService service = new ConfigurationServiceDataService(dataMock.Object);
        TestConfiguration configuration = new TestConfiguration();

        await service.Load(configuration);

        configuration.StringValue.Should().Be("loaded-value");
        configuration.AnotherString.Should().Be("loaded-other");
    }

    [Test]
    public async Task Save_SkipsReadOnlyProperties()
    {
        IEnumerable<SettingData> captured = Enumerable.Empty<SettingData>();
        Mock<IDataService> dataMock = new Mock<IDataService>();
        dataMock.Setup(d => d.SaveSettings(It.IsAny<IEnumerable<SettingData>>()))
            .Callback<IEnumerable<SettingData>>(settings => captured = settings.ToArray())
            .Returns(Task.CompletedTask);

        ConfigurationServiceDataService service = new ConfigurationServiceDataService(dataMock.Object);

        await service.Save(new TestConfiguration());

        captured.Select(s => s.Key).Should().NotContain(nameof(TestConfiguration.ReadOnlyValue));
    }
}
