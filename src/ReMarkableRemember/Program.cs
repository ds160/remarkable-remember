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
        AppBuilder.Configure<App>()
                  .UsePlatformDetect()
                  .WithInterFont()
                  .LogToTrace()
                  .UseReactiveUI(_ => { })
                  .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
    }
}
