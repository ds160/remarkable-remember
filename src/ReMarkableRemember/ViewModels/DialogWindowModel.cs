using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using ReactiveUI;
using ReactiveUI.Binding;
using ReactiveUI.Primitives;

namespace ReMarkableRemember.ViewModels;

public abstract class DialogWindowModel : ViewModelBase
{
    protected DialogWindowModel(String title, String textClose, String? textCancel = null)
    {
        this.CommandCancel = ReactiveCommand.CreateFromTask(this.OnCancel);
        this.CommandClose = ReactiveCommand.CreateFromTask(this.OnClose);

        this.CopyToClipboard = new Interaction<String, Boolean>();
        this.OpenFilePicker = new Interaction<FilePickerOpenOptions, IEnumerable<String>?>();
        this.OpenFolderPicker = new Interaction<String, String?>();

        this.TextCancel = textCancel;
        this.TextClose = textClose;
        this.Title = title;
    }

    public ReactiveCommand<RxVoid, Boolean> CommandCancel { get; }

    public ReactiveCommand<RxVoid, Boolean> CommandClose { get; }

    public Object Content { get { return this; } }

    public Interaction<String, Boolean> CopyToClipboard { get; }

    public Interaction<FilePickerOpenOptions, IEnumerable<String>?> OpenFilePicker { get; }

    public Interaction<String, String?> OpenFolderPicker { get; }

    public String? TextCancel { get; }

    public String TextClose { get; }

    public String Title { get; }

    protected virtual Task<Boolean> OnCancel()
    {
        return Task.FromResult(false);
    }

    protected virtual Task<Boolean> OnClose()
    {
        return Task.FromResult(true);
    }
}
