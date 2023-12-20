using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using ReactiveUI;
using ReMarkableRemember.Helper;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.ViewModels;

internal sealed class MainWindowViewModel : ViewModelBase, IDisposable
{
    private const Int32 CONNECTION_STATE_DELAY = 1;

    private TabletConnectionError? connectionState;
    private readonly Controller controller;
    private HierarchicalTreeDataGridSource<Item>? treeSource;

    public MainWindowViewModel(String dataSource)
    {
        this.controller = new Controller(dataSource);

        IObservable<Boolean>? canExecuteCommand = null;

        this.CommandHandWritingRecognition = ReactiveCommand.CreateFromTask(this.HandWritingRecognition, canExecuteCommand);
        this.CommandRefresh = ReactiveCommand.CreateFromTask(this.Refresh);
        this.CommandSync = ReactiveCommand.CreateFromTask(this.Sync, canExecuteCommand);

        _ = this.UpdateConnectionState();
    }

    public void Dispose()
    {
        this.controller.Dispose();

        GC.SuppressFinalize(this);
    }

    private async Task HandWritingRecognition()
    {
        Item? selectedItem = this.TreeSource?.RowSelection?.SelectedItem;
        if (selectedItem != null)
        {
            // TODO: Language and ResultDialog
            String text = await this.controller.HandWritingRecognition(selectedItem, "de_DE").ConfigureAwait(false);
            throw new MyScriptException(text);
        }
    }

    private async Task Refresh()
    {
        this.TreeSource = null;

        IEnumerable<Item> items = await this.controller.GetItems().ConfigureAwait(false);

        this.TreeSource = new HierarchicalTreeDataGridSource<Item>(items.Where(item => !item.Trashed))
        {
            Columns =
            {
                new HierarchicalExpanderColumn<Item>(new TextColumn<Item, String>("Name", item => item.Name), item => item.Collection),
                new TextColumn<Item, String>("Modified", item => item.Modified.ToDisplayString()),
                new TextColumn<Item, String>("Sync Path", item => item.SyncPath),
                new TextColumn<Item, String>("Sync Hint", item => item.SyncHint.HasValue ? item.SyncHint.Value.ToString() : null),
                new TextColumn<Item, String>("Last Sync", item => item.Sync != null ? item.Sync.Date.ToDisplayString() : null),
                new TextColumn<Item, String>("Backup Hint", item => item.BackupHint.HasValue ? item.BackupHint.Value.ToString() : null),
                new TextColumn<Item, String>("Last Backup", item => item.Backup.HasValue ? item.Backup.Value.ToDisplayString() : null),
                new TextColumn<Item, String>("ID", item => item.Id),
            }
        };

        this.TreeSource?.Sort(new Comparison<Item>((itemA, itemB) =>
        {
            Int32 collectionA = (itemA.Collection == null) ? 1 : 0;
            Int32 collectionB = (itemB.Collection == null) ? 1 : 0;
            Int32 collectionCompareResult = collectionA - collectionB;

            return (collectionCompareResult != 0)
                ? collectionCompareResult
                : String.CompareOrdinal(itemA.Name, itemB.Name);
        }));
    }

    private async Task Sync()
    {
        Item? selectedItem = this.TreeSource?.RowSelection?.SelectedItem;
        if (selectedItem != null)
        {
            await this.controller.SyncItem(selectedItem).ConfigureAwait(false);
        }
        // TODO: refresh
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
    public ICommand CommandRefresh { get; }
    public ICommand CommandSync { get; }

    public TabletConnectionError? ConnectionState
    {
        get { return this.connectionState; }
        private set { this.RaiseAndSetIfChanged(ref this.connectionState, value); }
    }

    public HierarchicalTreeDataGridSource<Item>? TreeSource
    {
        get { return this.treeSource; }
        private set { this.RaiseAndSetIfChanged(ref this.treeSource, value); }
    }
}
