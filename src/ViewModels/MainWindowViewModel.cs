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

        this.CommandHandWritingRecognition = ReactiveCommand.CreateFromTask(this.HandWritingRecognition, this.HandWritingRecognition_CanExecute());
        this.CommandProcess = ReactiveCommand.CreateFromTask(this.Process, this.Process_CanExecute());
        this.CommandRefresh = ReactiveCommand.CreateFromTask(this.Refresh, this.Refresh_CanExecute());

        _ = this.UpdateConnectionStatus();
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

    private IObservable<Boolean> HandWritingRecognition_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(state => state is null or (not TabletConnectionError.Unknown and not TabletConnectionError.SshNotConfigured and not TabletConnectionError.SshNotConnected));
        IObservable<Boolean> treeSelection = this.TreeSource.RowSelection!.WhenAnyValue(selection => selection.SelectedItem).Select(item => item != null && item.Collection == null);

        return Observable.CombineLatest(connectionStatus, treeSelection, (value1, value2) => value1 && value2);
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

    private IObservable<Boolean> Process_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(state => state is null);
        IObservable<Boolean> treeSelection = this.TreeSource.RowSelection!.WhenAnyValue(selection => selection.SelectedItem).Select(item => item != null);

        return Observable.CombineLatest(connectionStatus, treeSelection, (value1, value2) => value1 && value2);
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

    private IObservable<Boolean> Refresh_CanExecute()
    {
        return this.WhenAnyValue(vm => vm.ConnectionStatus).Select(state => state is null or (not TabletConnectionError.Unknown and not TabletConnectionError.SshNotConfigured and not TabletConnectionError.SshNotConnected));
    }

    private async Task UpdateConnectionStatus()
    {
        while (true)
        {
            this.ConnectionStatus = await this.controller.GetConnectionStatus().ConfigureAwait(true);
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(true);
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
