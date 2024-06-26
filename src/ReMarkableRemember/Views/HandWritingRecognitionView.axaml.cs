using Avalonia.ReactiveUI;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

public sealed partial class HandWritingRecognitionView : ReactiveUserControl<HandWritingRecognitionViewModel>
{
    public HandWritingRecognitionView()
    {
        this.InitializeComponent();
    }
}
