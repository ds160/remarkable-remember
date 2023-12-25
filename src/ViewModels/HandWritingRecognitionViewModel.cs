using System;

namespace ReMarkableRemember.ViewModels;

internal sealed class HandWritingRecognitionViewModel : ViewModelBase
{
    public HandWritingRecognitionViewModel(String text)
    {
        this.Text = text;
    }

    public String Text { get; }
}
