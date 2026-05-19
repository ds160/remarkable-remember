using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Services.ConfigurationService.Configuration;
using ReMarkableRemember.Services.ConfigurationService.Service;
using ReMarkableRemember.Services.ConfigurationService.Tests.Fakes;

namespace ReMarkableRemember.Services.ConfigurationService.Tests;

[TestFixture]
public sealed class ServiceBaseTests
{
    [Test]
    public void ServiceBase_OnConstruction_InstantiatesAndLoadsConfiguration()
    {
        Mock<IConfigurationService> serviceMock = new Mock<IConfigurationService>();
        serviceMock.Setup(s => s.Load(It.IsAny<ConfigurationBase>())).Returns(Task.CompletedTask);

        TestService service = new TestService(serviceMock.Object);

        service.Configuration.Should().NotBeNull();
        serviceMock.Verify(s => s.Load<ConfigurationBase>(service.Configuration), Times.Once);
    }

    [Test]
    public void ServiceBaseWithConfiguration_UsesProvidedInstance()
    {
        Mock<IConfigurationService> serviceMock = new Mock<IConfigurationService>();
        serviceMock.Setup(s => s.Load(It.IsAny<ConfigurationBase>())).Returns(Task.CompletedTask);

        TestConfiguration injected = new TestConfiguration();
        TestServiceWithExplicitConfig service = new TestServiceWithExplicitConfig(serviceMock.Object, injected);

        service.Configuration.Should().BeSameAs(injected);
        serviceMock.Verify(s => s.Load<ConfigurationBase>(injected), Times.Once);
    }

    private sealed class TestService : ServiceBase<TestConfiguration>
    {
        public TestService(IConfigurationService configurationService) : base(configurationService) { }
    }

    private sealed class TestServiceWithExplicitConfig : ServiceBaseWithConfiguration<TestConfiguration>
    {
        public TestServiceWithExplicitConfig(IConfigurationService configurationService, TestConfiguration configuration)
            : base(configurationService, configuration) { }
    }
}
