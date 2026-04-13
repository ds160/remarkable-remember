using System;
using Avalonia.Media;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Helper;

namespace ReMarkableRemember.ViewModels;

public sealed class MessageViewModel : DialogWindowModel
{
    private const String IMAGE_ERROR = "Error.svg";
    private const String IMAGE_QUESTION = "Question.svg";

    private MessageViewModel(String title, String message, String image, String textClose, String? textCancel = null)
        : base(title, textClose, textCancel)
    {
        this.Image = ImageLoader.Svg(image);
        this.Message = message;
    }

    internal static MessageViewModel Error(Exception exception)
    {
        return Error(exception.Message);
    }

    internal static MessageViewModel Error(String message)
    {
        return new MessageViewModel(Language.Current.ErrorTitle, message, IMAGE_ERROR, Language.Current.ButtonOK);
    }

    internal static MessageViewModel Question(String title, String message)
    {
        return new MessageViewModel(title, message, IMAGE_QUESTION, Language.Current.ButtonYes, Language.Current.ButtonNo);
    }

    public IImage Image { get; }

    public String Message { get; }
}
