using System;
using System.Reflection;
using Avalonia.Platform;
using Avalonia.Svg;

namespace ReMarkableRemember.ViewModels;

public sealed class MessageViewModel : DialogWindowModel
{
    private static readonly String? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    private MessageViewModel(String title, String message) : base(title, "Yes", "No")
    {
        this.Icon = LoadIcon("Question");
        this.Message = message;
    }

    private MessageViewModel(Exception exception) : base("Error", "OK")
    {
        this.Icon = LoadIcon("Error");
        this.Message = exception.Message;
    }

    internal static MessageViewModel Error(Exception exception)
    {
        return new MessageViewModel(exception);
    }

    internal static MessageViewModel Question(String title, String message)
    {
        return new MessageViewModel(title, message);
    }

    public SvgImage Icon { get; }

    public String Message { get; }

    private static SvgImage LoadIcon(String svg)
    {
        return new SvgImage() { Source = SvgSource.Load(AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/{svg}.svg"))) };
    }
}
