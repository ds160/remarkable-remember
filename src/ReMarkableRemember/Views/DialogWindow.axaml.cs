using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

public sealed partial class DialogWindow : ReactiveWindow<DialogWindowModel>
{
    public DialogWindow()
    {
        this.InitializeComponent();
        this.WhenActivated(this.Subscribe);
    }

    private async Task CopyToClipboardHandler(InteractionContext<String, Boolean> context)
    {
        if (this.Clipboard != null)
        {
            await this.Clipboard.SetTextAsync(context.Input);
            context.SetOutput(true);
        }
        else
        {
            context.SetOutput(false);
        }
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

    private void Subscribe(Action<IDisposable> action)
    {
        if (this.ViewModel == null) { return; }

        action(this.ViewModel.CommandCancel.Subscribe(result => this.Close(result)));
        action(this.ViewModel.CommandClose.Subscribe(result => this.Close(result)));
        action(this.ViewModel.CopyToClipboard.RegisterHandler(this.CopyToClipboardHandler));
        action(this.ViewModel.OpenFilePicker.RegisterHandler(this.OpenFilePickerHandler));
        action(this.ViewModel.OpenFolderPicker.RegisterHandler(this.OpenFolderPickerHandler));
    }
}
