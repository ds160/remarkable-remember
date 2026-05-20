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

    private static void ExceptionHandler(Exception exception)
    {
        String date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture);
        String logFilePath = FileSystem.CreateApplicationDataFilePath("logs.txt");
        File.AppendAllText(logFilePath, $"{date}|ERROR|{exception}{Environment.NewLine}");

        Window? mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow?.IsVisible == true)
        {
            DialogWindow dialogWindow = new DialogWindow() { DataContext = MessageViewModel.Error(exception) };
            dialogWindow.ShowDialog(mainWindow);
        }
    }
}
