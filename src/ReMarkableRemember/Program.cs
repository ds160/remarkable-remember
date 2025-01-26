using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using NLog;
using ReMarkableRemember.Common.FileSystem;

namespace ReMarkableRemember;

public sealed class Program
{
    [STAThread]
    public static void Main(String[] args)
    {
        LogManager.Setup()
                  .LoadConfiguration(builder => builder.ForLogger().WriteToFile(FileSystem.CreateApplicationDataFilePath("logs.txt")));

        AppBuilder.Configure<App>()
                  .UsePlatformDetect()
                  .WithInterFont()
                  .LogToTrace()
                  .UseReactiveUI()
                  .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
    }
}
