using System;
using System.Collections.Generic;
using System.Reactive;
using Avalonia.Platform.Storage;
using ReactiveUI;

namespace ReMarkableRemember.ViewModels;

public abstract class DialogWindowModel : ViewModelBase
{
    protected DialogWindowModel(String title, String textClose = "Close", Boolean showCancel = false)
    {
        this.CommandCancel = ReactiveCommand.Create(() => { return false; });
        this.CommandClose = ReactiveCommand.Create(() => { return true; });

        this.OpenFilePicker = new Interaction<FilePickerOpenOptions, IEnumerable<String>?>();
        this.OpenFolderPicker = new Interaction<String, String?>();

        this.ShowCancel = showCancel;
        this.TextClose = textClose;
        this.Title = title;
    }

    public ReactiveCommand<Unit, Boolean> CommandCancel { get; }

    public ReactiveCommand<Unit, Boolean> CommandClose { get; }

    public Object Content { get { return this; } }

    public Interaction<FilePickerOpenOptions, IEnumerable<String>?> OpenFilePicker { get; }

    public Interaction<String, String?> OpenFolderPicker { get; }

    public Boolean ShowCancel { get; }

    public String TextClose { get; }

    public String Title { get; }
}
