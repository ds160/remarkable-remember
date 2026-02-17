using ReactiveUI.Avalonia;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

public sealed partial class HandwritingRecognitionView : ReactiveUserControl<HandwritingRecognitionViewModel>
{
    public HandwritingRecognitionView()
    {
        this.InitializeComponent();
    }
}
