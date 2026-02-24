using System;
using System.Linq;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Avalonia.Splat;
using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.HandWritingRecognitionService;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.ViewModels;
using Splat;

namespace ReMarkableRemember;

public static class DependencyManager
{
    public static T GetRequired<T>()
    {
        return AppLocator.Current.GetService<T>() ?? throw new ArgumentException($"{typeof(T)} is not registered.");
    }

    public static AppBuilder UseReactiveUIWithDependencyManager(this AppBuilder builder, String[] args)
    {
        return builder.UseReactiveUIWithMicrosoftDependencyResolver(container => container
            .AddSingleton<IConfigurationService, ConfigurationServiceDataService>()
            .AddSingleton<IDataService>(DataServiceSqlite.Create(args?.FirstOrDefault()))
            .AddSingleton<IHandWritingRecognitionService, HandWritingRecognitionServiceMyScript>()
            .AddSingleton<ITabletService, TabletService>()
            .AddSingleton<MainWindowModel>(), null);
    }
}
