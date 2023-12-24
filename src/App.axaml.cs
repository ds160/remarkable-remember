using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
            String dataSource = GetDataSource(desktop.Args);
            desktop.MainWindow = new MainWindow() { DataContext = new MainWindowViewModel(dataSource) };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static String GetDataSource(String[]? args)
    {
        return args?.Length == 1 && File.Exists(args[0])
            ? args[0]
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(ReMarkableRemember), "database.db");
    }
}
