using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReMarkableRemember.Helper;
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
            String dataSource = GetDataSource(desktop.Args?.FirstOrDefault());
            desktop.MainWindow = new MainWindow() { DataContext = new MainWindowModel(dataSource) };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static String GetDataSource(String? arg)
    {
        if (File.Exists(arg)) { return arg; }

        String dataSource = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(ReMarkableRemember), "database.db");
        FileSystem.CreateDirectory(dataSource);
        return dataSource;
    }
}
