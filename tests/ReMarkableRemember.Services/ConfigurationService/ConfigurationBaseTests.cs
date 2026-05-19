using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Services.ConfigurationService.Configuration;
using ReMarkableRemember.Services.ConfigurationService.Tests.Fakes;

namespace ReMarkableRemember.Services.ConfigurationService.Tests;

[TestFixture]
public sealed class ConfigurationBaseTests
{
    [Test]
    public void GetPrefix_ReturnsConstructorPrefix()
    {
        TestConfiguration configuration = new TestConfiguration();

        ((IConfiguration)configuration).GetPrefix().Should().Be("TestPrefix");
    }

    [Test]
    public async Task Save_BeforeLoad_ThrowsInvalidOperationException()
    {
        TestConfiguration configuration = new TestConfiguration();

        Func<Task> act = configuration.Save;

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task Save_AfterLoad_DelegatesToConfigurationService()
    {
        Mock<IConfigurationService> serviceMock = new Mock<IConfigurationService>();
        serviceMock.Setup(s => s.Load(It.IsAny<ConfigurationBase>())).Returns(Task.CompletedTask);
        serviceMock.Setup(s => s.Save(It.IsAny<ConfigurationBase>())).Returns(Task.CompletedTask);

        TestConfiguration configuration = new TestConfiguration();
        configuration.Load(serviceMock.Object);

        await configuration.Save();

        serviceMock.Verify(s => s.Save<ConfigurationBase>(configuration), Times.Once);
    }

    [Test]
    public void Load_CallsConfigurationServiceLoad()
    {
        List<IConfiguration> received = new List<IConfiguration>();
        Mock<IConfigurationService> serviceMock = new Mock<IConfigurationService>();
        serviceMock.Setup(s => s.Load(It.IsAny<ConfigurationBase>()))
            .Callback<ConfigurationBase>(received.Add)
            .Returns(Task.CompletedTask);

        TestConfiguration configuration = new TestConfiguration();
        configuration.Load(serviceMock.Object);

        received.Should().ContainSingle().Which.Should().BeSameAs(configuration);
    }
}
