using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;

namespace ReMarkableRemember.ViewModels;

public sealed class HandwritingRecognitionViewModel : DialogWindowModel
{
    private readonly String originalText;
    private Boolean removeLineEndings;
    private String text;

    public HandwritingRecognitionViewModel(String text) : base("Handwriting Recognition", "Close")
    {
        this.CommandCopyTextToClipboard = ReactiveCommand.CreateFromTask(this.CopyTextToClipboard);

        this.originalText = text;
        this.removeLineEndings = false;
        this.text = text;

        this.WhenAnyValue(vm => vm.RemoveLineEndings).Subscribe(value => this.Text = value ? this.originalText.ReplaceLineEndings(" ") : this.originalText);
    }

    private async Task CopyTextToClipboard()
    {
        await this.CopyToClipboard.Handle(this.Text);
    }

    public ICommand CommandCopyTextToClipboard { get; }

    public Boolean RemoveLineEndings
    {
        get { return this.removeLineEndings; }
        set { this.RaiseAndSetIfChanged(ref this.removeLineEndings, value); }
    }

    public String Text
    {
        get { return this.text; }
        private set { this.RaiseAndSetIfChanged(ref this.text, value); }
    }
}
