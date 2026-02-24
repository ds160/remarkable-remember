using System;
using Avalonia;
using Avalonia.Controls;

namespace ReMarkableRemember;

public sealed class Program
{
    [STAThread]
    public static void Main(String[] args)
    {
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUIWithDependencyManager(args)
            .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
    }
}
