using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace ReMarkableRemember;

internal sealed class Program
{
    [STAThread]
    public static void Main(String[] args)
    {
        AppBuilder.Configure<App>()
                  .UsePlatformDetect()
                  .WithInterFont()
                  .LogToTrace()
                  .UseReactiveUI()
                  .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
    }
}
