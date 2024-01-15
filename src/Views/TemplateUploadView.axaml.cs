using Avalonia.ReactiveUI;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

public sealed partial class TemplateUploadView : ReactiveUserControl<TemplateUploadViewModel>
{
    public TemplateUploadView()
    {
        this.InitializeComponent();
    }
}
