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
            ServiceCollection services = new ServiceCollection();

            services.AddSingleton<IConfigurationService, ConfigurationServiceDataService>();
            services.AddSingleton<IDataService>(new DataServiceSqlite(desktop.Args?.FirstOrDefault()));
            services.AddSingleton<IHandWritingRecognitionService, HandWritingRecognitionServiceMyScript>();
            services.AddSingleton<ITabletService, TabletService>();

            desktop.MainWindow = new MainWindow() { DataContext = new MainWindowModel(services.BuildServiceProvider()) };
            this.DataContext = desktop.MainWindow.DataContext;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
