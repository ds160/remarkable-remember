using System;
using Avalonia.Svg;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Helper;

namespace ReMarkableRemember.ViewModels;

public sealed class MessageViewModel : DialogWindowModel
{
    private static readonly SvgImage imageError;
    private static readonly SvgImage imageQuestion;

    static MessageViewModel()
    {
        imageError = ImageLoader.Svg("Error.svg");
        imageQuestion = ImageLoader.Svg("Question.svg");
    }

    private MessageViewModel(String title, String message, SvgImage image, String textClose, String? textCancel = null)
        : base(title, textClose, textCancel)
    {
        this.Image = image;
        this.Message = message;
    }

    internal static MessageViewModel Error(Exception exception)
    {
        return Error(exception.Message);
    }

    internal static MessageViewModel Error(String message)
    {
        return new MessageViewModel(Language.Current.ErrorTitle, message, imageError, Language.Current.ButtonOK);
    }

    internal static MessageViewModel Question(String title, String message)
    {
        return new MessageViewModel(title, message, imageQuestion, Language.Current.ButtonYes, Language.Current.ButtonNo);
    }

    public SvgImage Image { get; }

    public String Message { get; }
}
