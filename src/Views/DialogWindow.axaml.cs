using System;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

internal sealed partial class DialogWindow : ReactiveWindow<DialogWindowModel>
{
    public DialogWindow()
    {
        this.InitializeComponent();
        this.WhenActivated(action => { if (this.ViewModel != null) { action(this.ViewModel.CommandClose.Subscribe(result => this.Close(result))); } });
    }
}
