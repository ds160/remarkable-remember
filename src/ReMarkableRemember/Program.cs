using System;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI.Avalonia;
using ReMarkableRemember.Helper;
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
        Log.Exception(exception);

        IClassicDesktopStyleApplicationLifetime? desktopApp = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        if (desktopApp?.MainWindow?.IsVisible == true)
        {
            DialogWindow dialog = new DialogWindow() { DataContext = MessageViewModel.Error(exception) };
            await dialog.ShowDialog(desktopApp.MainWindow).ConfigureAwait(true);
        }
    }
}
