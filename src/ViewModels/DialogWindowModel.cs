using System;
using System.Reactive;
using ReactiveUI;

namespace ReMarkableRemember.ViewModels;

public abstract class DialogWindowModel : ViewModelBase
{
    protected DialogWindowModel(String title, String textClose = "Close")
    {
        this.CommandClose = ReactiveCommand.Create(this.Close);
        this.TextClose = textClose;
        this.Title = title;
    }

    protected virtual Boolean Close()
    {
        return true;
    }

    public ReactiveCommand<Unit, Boolean> CommandClose { get; }

    public Object Content { get { return this; } }

    public String TextClose { get; }

    public String Title { get; }
}
