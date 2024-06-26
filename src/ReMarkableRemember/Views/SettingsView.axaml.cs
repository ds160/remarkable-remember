using Avalonia.ReactiveUI;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

public sealed partial class SettingsView : ReactiveUserControl<SettingsViewModel>
{
    public SettingsView()
    {
        this.InitializeComponent();
    }
}
