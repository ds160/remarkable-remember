using ReactiveUI.Avalonia;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

public sealed partial class TemplatesView : ReactiveUserControl<TemplatesViewModel>
{
    public TemplatesView()
    {
        this.InitializeComponent();
    }
}
