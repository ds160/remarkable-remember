using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase, IDisposable
{
    private const Int32 CONNECTION_STATE_DELAY = 1;

    private TabletConnectionError? connectionState;
    private readonly Controller controller;

    public MainWindowViewModel(String dataSource)
    {
        this.controller = new Controller(dataSource);

        this.CommandHandWritingRecognition = ReactiveCommand.CreateFromTask(this.controller.HandWritingRecognition);
        this.CommandSync = ReactiveCommand.CreateFromTask(this.Sync);

        _ = this.UpdateConnectionState();
    }

    public void Dispose()
    {
        this.controller.Dispose();

        GC.SuppressFinalize(this);
    }

    private async Task Sync()
    {
        IEnumerable<Controller.Item> items = await this.controller.GetItems().ConfigureAwait(false);
        await Task.WhenAll(items.Select(this.controller.SyncItem)).ConfigureAwait(false);
    }

    private async Task UpdateConnectionState()
    {
        while (true)
        {
            this.ConnectionState = await this.controller.GetConnectionStatus().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(CONNECTION_STATE_DELAY)).ConfigureAwait(false);
        }
    }

    public ICommand CommandHandWritingRecognition { get; }
    public ICommand CommandSync { get; }

    public TabletConnectionError? ConnectionState
    {
        get { return this.connectionState; }
        private set { this.RaiseAndSetIfChanged(ref this.connectionState, value); }
    }
}
