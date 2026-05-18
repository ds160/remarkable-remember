using Microsoft.Extensions.DependencyInjection;
using ReMarkableRemember.Services.TabletService.Communication;
using ReMarkableRemember.Services.TabletService.Communication.Interfaces;
using ReMarkableRemember.Services.TabletService.Files;
using ReMarkableRemember.Services.TabletService.Files.Interfaces;

namespace ReMarkableRemember.Services.TabletService;

public static class DependencyInjection
{
    public static IServiceCollection UseSshForTabletService(this IServiceCollection services)
    {
        return services
            .AddSingleton<ITabletCommunication, TabletCommunication>()
            .AddSingleton<ITabletFileSerializer, TabletFileSerializer>()
            .AddSingleton<ITabletService, TabletService>();
    }
}
