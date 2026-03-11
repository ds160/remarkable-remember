using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.HandWritingRecognitionService;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.ViewModels;
using ReMarkableRemember.Views;

namespace ReMarkableRemember;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
        {
            Object dataContext = CreateDataContext(desktopApp.Args);
            desktopApp.MainWindow = new MainWindow() { DataContext = dataContext };
            this.DataContext = dataContext;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static MainWindowModel CreateDataContext(String[]? args)
    {
        IServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton<IConfigurationService, ConfigurationServiceDataService>()
            .AddSingleton<IDataService>(DataServiceSqlite.Create(args?.FirstOrDefault()))
            .AddSingleton<IHandWritingRecognitionService, HandWritingRecognitionServiceMyScript>()
            .AddSingleton<ITabletService, TabletService>()
            .AddSingleton<MainWindowModel>()
            .BuildServiceProvider();

        return serviceProvider.GetRequiredService<MainWindowModel>();
    }
}
