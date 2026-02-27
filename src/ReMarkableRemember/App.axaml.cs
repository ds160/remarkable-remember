using System;
using System.Globalization;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReMarkableRemember.Common.FileSystem;
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
            Object dataContext = DependencyManager.Resolve<MainWindowModel>();
            desktop.MainWindow = new MainWindow() { DataContext = dataContext };
            this.DataContext = dataContext;
        }

        base.OnFrameworkInitializationCompleted();
    }

    internal static async void ExceptionHandler(Exception exception)
    {
        String logFilePath = FileSystem.CreateApplicationDataFilePath("logs.txt");
        File.AppendAllText(logFilePath, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture)}|ERROR|{exception.Source}|{exception}{Environment.NewLine}");

        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow?.IsVisible == true)
        {
            DialogWindow dialog = new DialogWindow() { DataContext = MessageViewModel.Error(exception) };
            await dialog.ShowDialog(desktop.MainWindow).ConfigureAwait(true);
        }
    }
}
