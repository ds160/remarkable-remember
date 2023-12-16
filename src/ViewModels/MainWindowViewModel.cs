using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase, IDisposable
{
    private const Int32 CONNECTION_STATE_DELAY = 1;

    private readonly Controller controller;
    private String? tabletConnectionMessage;

    public MainWindowViewModel()
    {
        this.controller = new Controller();

        this.CommandSync = ReactiveCommand.CreateFromTask(this.controller.Sync);

        _ = this.UpdateConnectionState();
    }

    public void Dispose()
    {
        this.controller.Dispose();

        GC.SuppressFinalize(this);
    }

    private async Task UpdateConnectionState()
    {
        while (true)
        {
            this.ConnectionState = await this.controller.GetConnectionStatus().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(CONNECTION_STATE_DELAY)).ConfigureAwait(false);
        }
    }

    public ICommand CommandSync { get; }

    public String? ConnectionState
    {
        get { return this.tabletConnectionMessage; }
        private set { this.RaiseAndSetIfChanged(ref this.tabletConnectionMessage, value); }
    }
}
