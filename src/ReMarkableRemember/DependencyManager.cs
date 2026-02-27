using System;
using System.Linq;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Avalonia;
using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.HandWritingRecognitionService;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember;

public static class DependencyManager
{
    private static IServiceProvider? serviceProvider;

    public static T Resolve<T>() where T : notnull
    {
        if (serviceProvider is null) { throw new InvalidOperationException(); }

        return serviceProvider.GetRequiredService<T>();
    }

    public static AppBuilder UseReactiveUIWithDependencyManager(this AppBuilder builder, String[] args)
    {
        serviceProvider = new ServiceCollection()
            .AddSingleton<IConfigurationService, ConfigurationServiceDataService>()
            .AddSingleton<IDataService>(DataServiceSqlite.Create(args?.FirstOrDefault()))
            .AddSingleton<IHandWritingRecognitionService, HandWritingRecognitionServiceMyScript>()
            .AddSingleton<ITabletService, TabletService>()
            .AddSingleton<MainWindowModel>()
            .BuildServiceProvider();

        return builder.UseReactiveUI(_ => { });
    }
}
