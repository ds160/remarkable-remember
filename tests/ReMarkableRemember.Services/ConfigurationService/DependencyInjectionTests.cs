using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Services.DataService;

namespace ReMarkableRemember.Services.ConfigurationService.Tests;

[TestFixture]
public sealed class DependencyInjectionTests
{
    [Test]
    public void UseDataServiceForConfigurationService_RegistersConfigurationServiceAsSingleton()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IDataService>());

        services.UseDataServiceForConfigurationService();

        ServiceProvider provider = services.BuildServiceProvider();
        IConfigurationService first = provider.GetRequiredService<IConfigurationService>();
        IConfigurationService second = provider.GetRequiredService<IConfigurationService>();

        first.Should().BeSameAs(second);
    }
}
