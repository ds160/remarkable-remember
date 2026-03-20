using System;
using System.Reflection;
using Avalonia.Platform;
using Avalonia.Svg;

namespace ReMarkableRemember.ViewModels;

public sealed class MessageViewModel : DialogWindowModel
{
    private enum Icon
    {
        Error,
        Question
    }

    private static readonly String? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    private MessageViewModel(String title, String message, Icon icon, String textClose, String? textCancel = null)
        : base(title, textClose, textCancel)
    {
        this.Image = LoadImage(icon);
        this.Message = message;
    }

    internal static MessageViewModel Error(Exception exception)
    {
        return Error(exception.Message);
    }

    internal static MessageViewModel Error(String message)
    {
        return new MessageViewModel("Error", message, Icon.Error, "OK");
    }

    internal static MessageViewModel Question(String title, String message)
    {
        return new MessageViewModel(title, message, Icon.Question, "Yes", "No");
    }

    public SvgImage Image { get; }

    public String Message { get; }

    private static SvgImage LoadImage(Icon icon)
    {
        return new SvgImage() { Source = SvgSource.Load(AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/{icon}.svg"))) };
    }
}
