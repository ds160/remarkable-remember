using System;
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ReMarkableRemember.ViewModels;

public sealed class MessageViewModel : DialogWindowModel
{
    private static readonly String? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    private MessageViewModel(String title, String message) : base(title, "Yes", "No")
    {
        this.Icon = new Bitmap(AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/Question.png")));
        this.Message = message;
    }

    private MessageViewModel(Exception exception) : base("Error", "OK")
    {
        this.Icon = new Bitmap(AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/Error.png")));
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

    public Bitmap Icon { get; }

    public String Message { get; }
}
