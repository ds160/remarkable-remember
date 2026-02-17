using ReactiveUI.Avalonia;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

public sealed partial class AboutView : ReactiveUserControl<AboutViewModel>
{
    public AboutView()
    {
        this.InitializeComponent();
    }
}
