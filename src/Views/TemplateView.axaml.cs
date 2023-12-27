using Avalonia.ReactiveUI;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

public sealed partial class TemplateView : ReactiveUserControl<TemplateViewModel>
{
    public TemplateView()
    {
        this.InitializeComponent();
    }
}
