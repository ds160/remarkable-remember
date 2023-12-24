using System;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;

namespace ReMarkableRemember.Views;

internal sealed class MainWindowExceptionHandler : IObserver<Exception>
{
    private readonly MainWindow owner;

    public MainWindowExceptionHandler(MainWindow owner)
    {
        this.owner = owner;
    }

    public void OnNext(Exception value)
    {
        this.ShowError(value);
    }

    public void OnError(Exception error)
    {
        this.ShowError(error);
    }

    public void OnCompleted()
    {
    }

    private async void ShowError(Exception exception)
    {
        IMsBox<ButtonResult> dialog = MessageBoxManager.GetMessageBoxStandard("Error", exception.Message, ButtonEnum.Ok, Icon.Error, WindowStartupLocation.CenterOwner);
        await dialog.ShowWindowDialogAsync(this.owner).ConfigureAwait(true);
    }
}
