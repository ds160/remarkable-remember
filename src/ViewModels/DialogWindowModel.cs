using System;
using System.Reactive;
using ReactiveUI;

namespace ReMarkableRemember.ViewModels;

public abstract class DialogWindowModel : ViewModelBase
{
    protected DialogWindowModel()
    {
        this.CommandClose = ReactiveCommand.Create(() => true);
    }

    public ReactiveCommand<Unit, Boolean> CommandClose { get; }

    public Object Content { get { return this; } }

    public abstract String Title { get; }
}
