using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.HandWritingRecognition;
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
        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ServiceProvider services = new ServiceCollection()
                .AddSingleton<IConfigurationService, ConfigurationServiceDataService>()
                .AddSingleton<IDataService>(DataServiceSqlite.Create(desktop.Args?.FirstOrDefault()))
                .AddSingleton<IHandWritingRecognitionService, HandWritingRecognitionServiceMyScript>()
                .AddSingleton<ITabletService, TabletService>()
                .AddSingleton<MainWindowModel>()
                .BuildServiceProvider();

            Object dataContext = services.GetRequiredService<MainWindowModel>();
            desktop.MainWindow = new MainWindow() { DataContext = dataContext };
            this.DataContext = dataContext;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
