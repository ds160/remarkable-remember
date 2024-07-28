using Avalonia.ReactiveUI;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

public sealed partial class HandwritingRecognitionView : ReactiveUserControl<HandwritingRecognitionViewModel>
{
    public HandwritingRecognitionView()
    {
        this.InitializeComponent();
    }
}
