using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using ReactiveUI;
using ReMarkableRemember.Helper;
using ReMarkableRemember.Models;
using ReMarkableRemember.Templates;

namespace ReMarkableRemember.ViewModels;

internal sealed class MainWindowViewModel : ViewModelBase, IDisposable
{
    private TabletConnectionError? connectionStatus;
    private readonly Controller controller;

    public MainWindowViewModel(String dataSource)
    {
        this.controller = new Controller(dataSource);

        this.TreeSource = new HierarchicalTreeDataGridSource<Item>(new List<Item>())
        {
            Columns =
            {
                new HierarchicalExpanderColumn<Item>(new TextColumn<Item, String>("Name", item => item.Name), item => item.Collection),
                new TextColumn<Item, String>("Modified", item => item.Modified.ToDisplayString()),
                new TextColumn<Item, String>("Sync Path", item => item.SyncPath),
                new TemplateColumn<Item>("Sync Information", new TreeDataGridItemHintColumn(item => item.Sync?.Date, item => item.SyncHint)),
                new TemplateColumn<Item>("Backup Information", new TreeDataGridItemHintColumn(item => item.Backup, item => item.BackupHint)),
            }
        };

        this.CommandHandWritingRecognition = ReactiveCommand.CreateFromTask(this.HandWritingRecognition, this.HandWritingRecognition_CanExecute());
        this.CommandProcess = ReactiveCommand.CreateFromTask(this.Process, this.Process_CanExecute());
        this.CommandRefresh = ReactiveCommand.CreateFromTask(this.Refresh, this.Refresh_CanExecute());

        this.WhenAnyValue(vm => vm.ConnectionStatus).Subscribe(status => this.RaisePropertyChanged(nameof(this.ConnectionStatusText)));

        _ = this.UpdateConnectionStatus();
    }

    public void Dispose()
    {
        this.controller.Dispose();

        GC.SuppressFinalize(this);
    }

    private async Task HandWritingRecognition()
    {
        Item? selectedItem = this.TreeSource.RowSelection!.SelectedItem;
        if (selectedItem != null)
        {
            String text = await this.controller.HandWritingRecognition(selectedItem, "de_DE").ConfigureAwait(false);
            throw new MyScriptException(text);
        }
    }

    private IObservable<Boolean> HandWritingRecognition_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => status is null or (not TabletConnectionError.Unknown and not TabletConnectionError.SshNotConfigured and not TabletConnectionError.SshNotConnected));
        IObservable<Boolean> treeSelection = this.TreeSource.RowSelection!.WhenAnyValue(selection => selection.SelectedItem).Select(item => item != null && item.Collection == null);

        return Observable.CombineLatest(connectionStatus, treeSelection, (value1, value2) => value1 && value2);
    }

    private async Task Process()
    {
        Item? selectedItem = this.TreeSource.RowSelection!.SelectedItem;
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
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => status is null);
        IObservable<Boolean> treeSelection = this.TreeSource.RowSelection!.WhenAnyValue(selection => selection.SelectedItem).Select(item => item != null);

        return Observable.CombineLatest(connectionStatus, treeSelection, (value1, value2) => value1 && value2);
    }

    private async Task Refresh()
    {
        try
        {
            IEnumerable<Item> items = await this.controller.GetItems().ConfigureAwait(false);
            this.TreeSource.Items = items.Where(item => !item.Trashed).ToList();

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
        catch
        {
            this.TreeSource.Items = new List<Item>();

            throw;
        }
    }

    private IObservable<Boolean> Refresh_CanExecute()
    {
        return this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => status is null or (not TabletConnectionError.Unknown and not TabletConnectionError.SshNotConfigured and not TabletConnectionError.SshNotConnected));
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

    public String ConnectionStatusText
    {
        get
        {
            switch (this.ConnectionStatus)
            {
                case null: return "Connected";
                case TabletConnectionError.Unknown: return "Unknown connection error";
                case TabletConnectionError.SshNotConfigured: return "SSH protocol information are not configured or wrong";
                case TabletConnectionError.SshNotConnected: return "Not connected via WiFi or USB";
                case TabletConnectionError.UsbNotActived: return "USB web interface is not activated";
                case TabletConnectionError.UsbNotConnected: return "Not connected via USB";
                default: throw new NotImplementedException();
            }
        }
    }

    public HierarchicalTreeDataGridSource<Item> TreeSource { get; }
}
