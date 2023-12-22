using System;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

internal sealed partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        this.InitializeComponent();
        this.WhenActivated(action => { if (this.ViewModel != null) { action(this.ViewModel.ShowDialog.RegisterHandler(this.ShowDialogHandler)); } });

        RxApp.DefaultExceptionHandler = new MainWindowExceptionHandler(this);
    }

    private async Task ShowDialogHandler(InteractionContext<String, Boolean> interaction)
    {
        IMsBox<ButtonResult> dialog = MessageBoxManager.GetMessageBoxStandard(String.Empty, interaction.Input, ButtonEnum.Ok);
        ButtonResult result = await dialog.ShowWindowDialogAsync(this).ConfigureAwait(false);
        interaction.SetOutput(result == ButtonResult.Ok);
    }
}
