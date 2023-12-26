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
        this.WhenActivated(action => { if (this.ViewModel != null) { this.Subscribe(action, this.ViewModel); } });
    }

    private async Task OpenFilePickerHandler(InteractionContext<String, String?> context)
    {
        FilePickerOpenOptions options = new FilePickerOpenOptions() { AllowMultiple = false, Title = context.Input };
        IReadOnlyList<IStorageFile> files = await this.StorageProvider.OpenFilePickerAsync(options).ConfigureAwait(true);
        context.SetOutput(files?.Select(file => file.Path.AbsolutePath).Single());
    }

    private async Task OpenFolderPickerHandler(InteractionContext<String, String?> context)
    {
        FolderPickerOpenOptions options = new FolderPickerOpenOptions() { AllowMultiple = false, Title = context.Input };
        IReadOnlyList<IStorageFolder> folders = await this.StorageProvider.OpenFolderPickerAsync(options).ConfigureAwait(true);
        context.SetOutput(folders?.Select(folder => folder.Path.AbsolutePath).Single());
    }

    private void Subscribe(Action<IDisposable> action, DialogWindowModel viewModel)
    {
        action(viewModel.CommandCancel.Subscribe(result => this.Close(result)));
        action(viewModel.CommandClose.Subscribe(result => this.Close(result)));
        action(viewModel.OpenFilePicker.RegisterHandler(this.OpenFilePickerHandler));
        action(viewModel.OpenFolderPicker.RegisterHandler(this.OpenFolderPickerHandler));
    }
}
