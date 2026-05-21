using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReMarkableRemember.DependencyInjection;
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
            IServices services = DependencyInjection.Services.Create(desktopApp.Args);
            Object dataContext = MainWindowModel.Create(services);
            desktopApp.MainWindow = new MainWindow() { DataContext = dataContext };
            this.DataContext = dataContext;
        }
    }
}
