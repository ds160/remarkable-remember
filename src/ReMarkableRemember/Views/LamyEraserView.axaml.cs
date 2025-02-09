using Avalonia.ReactiveUI;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

public sealed partial class LamyEraserView : ReactiveUserControl<LamyEraserViewModel>
{
    public LamyEraserView()
    {
        this.InitializeComponent();
    }
}
