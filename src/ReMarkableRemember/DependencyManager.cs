using System;
using System.Linq;
using DryIoc;
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

    public static void Setup(Container container, String[] args)
    {
        container.Register<IConfigurationService, ConfigurationServiceDataService>(Reuse.Singleton);
        container.RegisterDelegate<IDataService>(_ => DataServiceSqlite.Create(args?.FirstOrDefault()), Reuse.Singleton);
        container.Register<IHandWritingRecognitionService, HandWritingRecognitionServiceMyScript>(Reuse.Singleton);
        container.Register<ITabletService, TabletService>(Reuse.Singleton);
        container.Register<MainWindowModel>(Reuse.Singleton);
    }
}
