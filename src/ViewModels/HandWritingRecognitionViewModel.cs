using System;

namespace ReMarkableRemember.ViewModels;

public sealed class HandWritingRecognitionViewModel : DialogWindowModel
{
    public HandWritingRecognitionViewModel(String text)
    {
        this.Text = text;
    }

    public String Text { get; }

    public override String Title { get { return "Hand Writing Recognition"; } }
}
