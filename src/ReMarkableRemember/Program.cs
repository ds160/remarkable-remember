using System;
using System.Globalization;
using System.IO;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI.Avalonia;
using ReMarkableRemember.Common.FileSystem;
using ReMarkableRemember.ViewModels;
using ReMarkableRemember.Views;

namespace ReMarkableRemember;

public sealed class Program
{
    [STAThread]
    public static void Main(String[] args)
    {
        try
        {
            AppBuilder
                .Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI(builder => builder.WithExceptionHandler(Observer.Create<Exception>(ExceptionHandler)))
                .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
        }
        catch (Exception ex)
        {
            ExceptionHandler(ex);
        }
    }

    private static async void ExceptionHandler(Exception exception)
    {
        String logFilePath = FileSystem.CreateApplicationDataFilePath("logs.txt");
        File.AppendAllText(logFilePath, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture)}|ERROR|{exception}{Environment.NewLine}");

        IClassicDesktopStyleApplicationLifetime? desktopApp = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        if (desktopApp?.MainWindow?.IsVisible == true)
        {
            DialogWindow dialog = new DialogWindow() { DataContext = MessageViewModel.Error(exception) };
            await dialog.ShowDialog(desktopApp.MainWindow).ConfigureAwait(true);
        }
    }
}
