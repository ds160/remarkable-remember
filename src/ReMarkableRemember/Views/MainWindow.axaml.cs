using System;
using System.Collections.Generic;
using System.Linq;
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
    }

    private async Task OpenFilePickerHandler(IInteractionContext<FilePickerOpenOptions, IEnumerable<String>?> context)
    {
        IReadOnlyList<IStorageFile> files = await this.StorageProvider.OpenFilePickerAsync(context.Input).ConfigureAwait(true);
        context.SetOutput(files?.Select(file => file.Path.LocalPath).ToArray());
    }

    private async Task OpenFolderPickerHandler(IInteractionContext<String, String?> context)
    {
        FolderPickerOpenOptions options = new FolderPickerOpenOptions() { AllowMultiple = false, Title = context.Input };
        IReadOnlyList<IStorageFolder> folders = await this.StorageProvider.OpenFolderPickerAsync(options).ConfigureAwait(true);
        context.SetOutput(folders?.Select(folder => folder.Path.LocalPath).SingleOrDefault());
    }

    private async Task OpenSaveFilePickerHandler(IInteractionContext<FilePickerSaveOptions, String?> context)
    {
        IStorageFile? file = await this.StorageProvider.SaveFilePickerAsync(context.Input).ConfigureAwait(true);
        context.SetOutput(file?.Path.LocalPath);
    }

    private async Task ShowDialogHandler(IInteractionContext<DialogWindowModel, Boolean> context)
    {
        DialogWindow dialog = new DialogWindow() { DataContext = context.Input };
        Boolean? result = await dialog.ShowDialog<Boolean?>(this).ConfigureAwait(true);
        context.SetOutput(result == true);
    }

    private void Subscribe(Action<IDisposable> action)
    {
        if (this.ViewModel == null) { return; }

        action(this.ViewModel.ShowDialog.RegisterHandler(this.ShowDialogHandler));
        action(this.ViewModel.OpenFilePicker.RegisterHandler(this.OpenFilePickerHandler));
        action(this.ViewModel.OpenFolderPicker.RegisterHandler(this.OpenFolderPickerHandler));
        action(this.ViewModel.OpenSaveFilePicker.RegisterHandler(this.OpenSaveFilePickerHandler));
    }
}
