using System;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI.Avalonia.Splat;

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
            .UseReactiveUIWithDryIoc(container => DependencyManager.Setup(container, args))
            .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
    }
}
