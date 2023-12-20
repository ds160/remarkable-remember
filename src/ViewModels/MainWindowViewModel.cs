using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    private TabletConnectionError? connectionStatus;
    private readonly Controller controller;
    private readonly ObservableCollection<Item> items;

    public MainWindowViewModel(String dataSource)
    {
        this.controller = new Controller(dataSource);
        this.items = new ObservableCollection<Item>();

        this.TreeSource = new HierarchicalTreeDataGridSource<Item>(this.items)
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

        IObservable<Boolean> sepp = this.TreeSource.RowSelection!.WhenAnyValue(selection => selection.SelectedItem).Select(item => item != null && item.Collection == null);
        // IObservable<Boolean> hugo = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(state => state == null);
        // this.WhenAnyValue(vm => vm.ConnectionState).Subscribe(state => Console.WriteLine(state));
        // IObservable<Boolean> combinedObservable = Observable.CombineLatest(sepp, hugo, (value1, value2) => value1 && value2);

        this.CommandHandWritingRecognition = ReactiveCommand.CreateFromTask(this.HandWritingRecognition, sepp);
        this.CommandProcess = ReactiveCommand.CreateFromTask(this.Process);
        this.CommandRefresh = ReactiveCommand.CreateFromTask(this.Refresh);

        _ = this.UpdateConnectionStatus().ConfigureAwait(false);
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

    private async Task Process()
    {
        Item? selectedItem = this.TreeSource?.RowSelection?.SelectedItem;
        if (selectedItem != null)
        {
            try
            {
                await this.controller.ProcessItem(selectedItem).ConfigureAwait(false);
            }
            finally
            {
                await this.Refresh().ConfigureAwait(false);
            }
        }
    }

    private async Task Refresh()
    {
        this.items.Clear();

        IEnumerable<Item> items = await this.controller.GetItems().ConfigureAwait(false);
        foreach (Item item in items.Where(item => !item.Trashed).ToArray())
        {
            this.items.Add(item);
        }

        this.TreeSource.Sort(new Comparison<Item>((itemA, itemB) =>
        {
            Int32 collectionA = (itemA.Collection == null) ? 1 : 0;
            Int32 collectionB = (itemB.Collection == null) ? 1 : 0;
            Int32 collectionCompareResult = collectionA - collectionB;

            return (collectionCompareResult != 0)
                ? collectionCompareResult
                : String.CompareOrdinal(itemA.Name, itemB.Name);
        }));
    }

    private async Task UpdateConnectionStatus()
    {
        while (true)
        {
            this.ConnectionStatus = await this.controller.GetConnectionStatus().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(CONNECTION_STATE_DELAY)).ConfigureAwait(false);
        }
    }

    public ICommand CommandHandWritingRecognition { get; }
    public ICommand CommandProcess { get; }
    public ICommand CommandRefresh { get; }

    public TabletConnectionError? ConnectionStatus
    {
        get { return this.connectionStatus; }
        private set { this.RaiseAndSetIfChanged(ref this.connectionStatus, value); }
    }

    public HierarchicalTreeDataGridSource<Item> TreeSource { get; }
}
