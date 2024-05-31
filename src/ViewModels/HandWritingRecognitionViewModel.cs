using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;

namespace ReMarkableRemember.ViewModels;

public sealed class HandWritingRecognitionViewModel : DialogWindowModel
{
    private readonly String originalText;
    private Boolean removeLineEndings;
    private String text;

    public HandWritingRecognitionViewModel(String text) : base("Hand Writing Recognition", "Close")
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

    public ReactiveCommand<Unit, Unit> CommandCopyTextToClipboard { get; }

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
