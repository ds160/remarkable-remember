using ReactiveUI.Avalonia;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

public sealed partial class MessageView : ReactiveUserControl<MessageViewModel>
{
    public MessageView()
    {
        this.InitializeComponent();
    }
}
