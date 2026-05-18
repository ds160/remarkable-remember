using Microsoft.Extensions.DependencyInjection;

namespace ReMarkableRemember.Services.ConfigurationService;

public static class DependencyInjection
{
    public static IServiceCollection UseDataServiceForConfigurationService(this IServiceCollection services)
    {
        return services.AddSingleton<IConfigurationService, ConfigurationServiceDataService>();
    }
}
