using System;

namespace ReMarkableRemember.ViewModels;

public sealed class HandWritingRecognitionViewModel : DialogWindowModel
{
    public HandWritingRecognitionViewModel(String text) : base("Hand Writing Recognition")
    {
        this.Text = text;
    }

    public String Text { get; }
}
