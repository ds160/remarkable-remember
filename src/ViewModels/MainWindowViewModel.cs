using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
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
                new TemplateColumn<Item>("Sync Information", new TemplateColumnTemplate(item => item.Sync?.Date, item => item.SyncHint)),
                new TemplateColumn<Item>("Backup Information", new TemplateColumnTemplate(item => item.Backup, item => item.BackupHint)),
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
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => status is null or (not TabletConnectionError.Unknown and not TabletConnectionError.SshNotConfigured and not TabletConnectionError.SshNotConnected));
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
                // TODO: Refresh
                // await this.Refresh().ConfigureAwait(false);
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

    public HierarchicalTreeDataGridSource<Item> TreeSource { get; }

    private sealed class TemplateColumnTemplate : IDataTemplate
    {
        private readonly Func<Item, DateTime?> getDateTime;
        private readonly Func<Item, Item.Hint?> getHint;

        public TemplateColumnTemplate(Func<Item, DateTime?> getDateTime, Func<Item, Item.Hint?> getHint)
        {
            this.getDateTime = getDateTime;
            this.getHint = getHint;
        }
        public Control? Build(Object? param)
        {
            Item item = param as Item ?? throw new ArgumentNullException(nameof(param));

            // TODO: IDataTemplate for Hint+Last and Indicator for any child hint
            DateTime? dateTime = this.getDateTime(item);
            Item.Hint? hint = this.getHint(item);

            StackPanel stackPanel = new StackPanel() { Margin = new Thickness(4.0), Orientation = Orientation.Horizontal, Spacing = 4.0 };
            stackPanel.Children.Add(new TextBlock() { Text = dateTime?.ToDisplayString() });
            stackPanel.Children.Add(new TextBlock() { Text = hint.HasValue ? hint.Value.ToString() : null });
            ToolTip.SetTip(stackPanel, hint.HasValue ? hint.Value.ToString() : null);
            return stackPanel;
        }

        public Boolean Match(Object? data)
        {
            return data is Item;
        }
    }
}
