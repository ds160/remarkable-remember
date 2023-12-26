using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

public sealed partial class MainWindow : ReactiveWindow<MainWindowModel>
{
    public MainWindow()
    {
        this.InitializeComponent();
        this.WhenActivated(action => { if (this.ViewModel != null) { action(this.ViewModel.ShowDialog.RegisterHandler(this.ShowDialogHandler)); } });

        RxApp.DefaultExceptionHandler = Observer.Create<Exception>(this.ShowExceptionDialog, this.ShowExceptionDialog);
    }

    private async Task ShowDialogHandler(InteractionContext<DialogWindowModel, Boolean> interaction)
    {
        DialogWindow dialog = new DialogWindow() { DataContext = interaction.Input };
        Boolean? result = await dialog.ShowDialog<Boolean?>(this).ConfigureAwait(true);
        interaction.SetOutput(result == true);
    }

    private async void ShowExceptionDialog(Exception exception)
    {
        DialogWindow dialog = new DialogWindow() { DataContext = new ExceptionViewModel(exception.Message) };
        await dialog.ShowDialog<Boolean?>(this).ConfigureAwait(true);
    }
}
