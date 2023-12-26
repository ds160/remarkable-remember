using System;
using ReMarkableRemember.ViewModels;

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
        DialogWindow dialog = new DialogWindow() { DataContext = new ExceptionViewModel(exception.Message) };
        await dialog.ShowDialog<Boolean?>(this.owner).ConfigureAwait(true);
    }
}
