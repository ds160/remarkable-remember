using Avalonia.ReactiveUI;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

public sealed partial class TemplatesRestoreView : ReactiveUserControl<TemplatesRestoreViewModel>
{
    public TemplatesRestoreView()
    {
        this.InitializeComponent();
    }
}
