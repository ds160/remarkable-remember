using System;
using System.Reactive;
using ReactiveUI;

namespace ReMarkableRemember.ViewModels;

internal sealed class DialogWindowModel : ViewModelBase
{
    public DialogWindowModel(String title, ViewModelBase content)
    {
        this.Content = content;
        this.Title = title;

        this.CommandClose = ReactiveCommand.Create(() => true);
    }

    public ReactiveCommand<Unit, Boolean> CommandClose { get; }

    public ViewModelBase Content { get; }
    public String Title { get; }
}
