using System;
using Avalonia.Media;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.DependencyInjection;

namespace ReMarkableRemember.ViewModels;

public sealed class MessageViewModel : DialogWindowModel
{
    private const String IMAGE_ERROR = "Messages/Error.svg";
    private const String IMAGE_QUESTION = "Messages/Question.svg";

    private MessageViewModel(String title, String message, IImage image, String textClose, String? textCancel = null)
        : base(title, textClose, textCancel)
    {
        this.Image = image;
        this.Message = message;
    }

    internal static MessageViewModel Error(Exception exception, IServices services)
    {
        return Error(exception.Message, services);
    }

    internal static MessageViewModel Error(String message, IServices services)
    {
        return new MessageViewModel(Language.Current.ErrorTitle, message, services.ImageLoader.Svg(IMAGE_ERROR), Language.Current.ButtonOK);
    }

    internal static MessageViewModel Question(String title, String message, IServices services)
    {
        return new MessageViewModel(title, message, services.ImageLoader.Svg(IMAGE_QUESTION), Language.Current.ButtonYes, Language.Current.ButtonNo);
    }

    public IImage Image { get; }

    public String Message { get; }
}
