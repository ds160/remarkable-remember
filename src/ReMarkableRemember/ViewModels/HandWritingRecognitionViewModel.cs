using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;

namespace ReMarkableRemember.ViewModels;

public sealed class HandwritingRecognitionViewModel : DialogWindowModel
{
    private readonly String originalText;

    public HandwritingRecognitionViewModel(String text) : base("Handwriting Recognition", "Close")
    {
        this.originalText = text;

        this.CommandCopyTextToClipboard = ReactiveCommand.CreateFromTask(this.CopyTextToClipboard);
        this.RemoveLineEndings = false;
        this.Text = text;

        this.WhenAnyValue(vm => vm.RemoveLineEndings).Subscribe(value => this.Text = value ? this.originalText.ReplaceLineEndings(" ") : this.originalText);
    }

    private async Task CopyTextToClipboard()
    {
        await this.CopyToClipboard.Handle(this.Text);
    }

    public ICommand CommandCopyTextToClipboard { get; }

    public Boolean RemoveLineEndings { get; set { this.RaiseAndSetIfChanged(ref field, value); } }

    public String Text { get; private set { this.RaiseAndSetIfChanged(ref field, value); } }
}
