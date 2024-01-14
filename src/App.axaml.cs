using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReMarkableRemember.Entities;
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
            String dataSource = DatabaseContextFactory.GetDataSource(desktop.Args?.FirstOrDefault());
            desktop.MainWindow = new MainWindow() { DataContext = new MainWindowModel(dataSource) };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
