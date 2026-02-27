using System;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI.Avalonia;

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
                .UseReactiveUI(_ => { })
                .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
        }
        catch (Exception ex)
        {
            App.ExceptionHandler(ex);
        }
    }
}
