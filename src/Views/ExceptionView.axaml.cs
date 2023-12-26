using Avalonia.ReactiveUI;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

public sealed partial class ExceptionView : ReactiveUserControl<ExceptionViewModel>
{
    public ExceptionView()
    {
        this.InitializeComponent();
    }
}
