using System;
using System.ComponentModel;
using Avalonia.Media;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.DependencyInjection;
using ReMarkableRemember.Images;

namespace ReMarkableRemember.ViewModels;

public sealed class MessageViewModel : DialogWindowModel
{
    private const String IMAGE_ERROR = "Messages/Error.svg";
    private const String IMAGE_QUESTION = "Messages/Question.svg";

    private static readonly ImageLoader imageLoader = new ImageLoader();

    private MessageViewModel(String title, String message, IImage image, String textClose, String? textCancel = null)
        : base(title, textClose, textCancel)
    {
        this.Image = image;
        this.Message = message;
    }

    public IImage Image { get; }

    public String Message { get; }

    [Browsable(false)]
    internal static MessageViewModel ApplicationException(Exception exception)
    {
        return ErrorCore(exception.Message, imageLoader);
    }

    public static MessageViewModel Error(Exception exception, IServices services)
    {
        return ErrorCore(exception.Message, services.ImageLoader);
    }

    public static MessageViewModel Error(String message, IServices services)
    {
        return ErrorCore(message, services.ImageLoader);
    }

    public static MessageViewModel Question(String title, String message, IServices services)
    {
        return QuestionCore(title, message, services.ImageLoader);
    }

    private static MessageViewModel ErrorCore(String message, IImageLoader imageLoader)
    {
        return new MessageViewModel(Language.Current.ErrorTitle, message, imageLoader.Svg(IMAGE_ERROR), Language.Current.ButtonOK);
    }

    private static MessageViewModel QuestionCore(String title, String message, IImageLoader imageLoader)
    {
        return new MessageViewModel(title, message, imageLoader.Svg(IMAGE_QUESTION), Language.Current.ButtonYes, Language.Current.ButtonNo);
    }
}
