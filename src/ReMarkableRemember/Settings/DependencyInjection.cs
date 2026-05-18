using Microsoft.Extensions.DependencyInjection;

namespace ReMarkableRemember.Settings;

public static class DependencyInjection
{
    public static IServiceCollection UseConfigurationServiceForSettingsService(this IServiceCollection services)
    {
        return services.AddSingleton<ISettingsService, SettingsService>();
    }
}
