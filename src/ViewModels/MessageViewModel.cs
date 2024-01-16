using System;
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ReMarkableRemember.ViewModels;

public sealed class MessageViewModel : DialogWindowModel
{
    private static readonly String? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    internal MessageViewModel(String title, String message) : base(title, "Yes", "No")
    {
        this.Icon = new Bitmap(AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/Question.png")));
        this.Message = message;
    }

    internal MessageViewModel(Exception exception) : base("Error", "OK")
    {
        this.Icon = new Bitmap(AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/Error.png")));
        this.Message = exception.Message;
    }

    public Bitmap Icon { get; }

    public String Message { get; }
}
