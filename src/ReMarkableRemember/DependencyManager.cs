using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.HandWritingRecognitionService;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember;

public sealed class DependencyManager
{
    private readonly IServiceProvider serviceProvider;

    public DependencyManager(String[]? args)
    {
        this.serviceProvider = new ServiceCollection()
            .AddSingleton<IConfigurationService, ConfigurationServiceDataService>()
            .AddSingleton<IDataService>(DataServiceSqlite.Create(args?.FirstOrDefault()))
            .AddSingleton<IHandWritingRecognitionService, HandWritingRecognitionServiceMyScript>()
            .AddSingleton<ITabletService, TabletService>()
            .AddSingleton<MainWindowModel>()
            .BuildServiceProvider();
    }

    public T Resolve<T>() where T : notnull
    {
        return this.serviceProvider.GetRequiredService<T>();
    }
}
