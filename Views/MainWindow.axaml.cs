using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

public sealed partial class MainWindow : ReactiveWindow<MainWindowModel>
{
    public MainWindow()
    {
        this.InitializeComponent();
        this.WhenActivated(this.Subscribe);

        RxApp.DefaultExceptionHandler = Observer.Create<Exception>(this.ShowExceptionDialog, this.ShowExceptionDialog);
    }

    private async Task OpenFilePickerHandler(InteractionContext<FilePickerOpenOptions, IEnumerable<String>?> context)
    {
        IReadOnlyList<IStorageFile> files = await this.StorageProvider.OpenFilePickerAsync(context.Input).ConfigureAwait(true);
        context.SetOutput(files?.Select(file => file.Path.LocalPath).ToArray());
    }

    private async Task OpenFolderPickerHandler(InteractionContext<String, String?> context)
    {
        FolderPickerOpenOptions options = new FolderPickerOpenOptions() { AllowMultiple = false, Title = context.Input };
        IReadOnlyList<IStorageFolder> folders = await this.StorageProvider.OpenFolderPickerAsync(options).ConfigureAwait(true);
        context.SetOutput(folders?.Select(folder => folder.Path.LocalPath).SingleOrDefault());
    }

    private async Task ShowDialogHandler(InteractionContext<DialogWindowModel, Boolean> context)
    {
        DialogWindow dialog = new DialogWindow() { DataContext = context.Input };
        Boolean? result = await dialog.ShowDialog<Boolean?>(this).ConfigureAwait(true);
        context.SetOutput(result == true);
    }

    private async void ShowExceptionDialog(Exception exception)
    {
        DialogWindow dialog = new DialogWindow() { DataContext = MessageViewModel.Error(exception) };
        await dialog.ShowDialog<Boolean?>(this).ConfigureAwait(true);
    }

    private void Subscribe(Action<IDisposable> action)
    {
        if (this.ViewModel == null) { return; }

        action(this.ViewModel.ShowDialog.RegisterHandler(this.ShowDialogHandler));
        action(this.ViewModel.OpenFilePicker.RegisterHandler(this.OpenFilePickerHandler));
        action(this.ViewModel.OpenFolderPicker.RegisterHandler(this.OpenFolderPickerHandler));
    }
}
