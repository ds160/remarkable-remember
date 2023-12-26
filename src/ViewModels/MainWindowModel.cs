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
using ReMarkableRemember.Models.Interfaces;
using ReMarkableRemember.Models.Stubs;
using ReMarkableRemember.Templates;

namespace ReMarkableRemember.ViewModels;

public sealed class MainWindowModel : ViewModelBase, IDisposable
{
    private TabletConnectionError? connectionStatus;
    private readonly IController controller;
    private Boolean hasItems;

    public MainWindowModel(String dataSource, Boolean noHardware)
    {
        this.connectionStatus = TabletConnectionError.SshNotConnected;
        this.controller = noHardware ? new ControllerStub(dataSource) : new Controller(dataSource);
        this.hasItems = false;

        this.ShowDialog = new Interaction<DialogWindowModel, Boolean>();
        this.TreeSource = new HierarchicalTreeDataGridSource<ItemViewModel>(new List<ItemViewModel>())
        {
            Columns =
            {
                new HierarchicalExpanderColumn<ItemViewModel>(new TextColumn<ItemViewModel, String>("Name", item => item.Name), item => item.Collection),
                new TextColumn<ItemViewModel, String>("Modified", item => item.Modified.ToDisplayString()),
                new TemplateColumn<ItemViewModel>(null, new ItemHintColumnTemplate(item => null, item => item.CombinedHint)),
                new TextColumn<ItemViewModel, String>("Sync Path", item => item.SyncPath),
                new TemplateColumn<ItemViewModel>("Sync Information", new ItemHintColumnTemplate(item => item.Sync, item => item.SyncHint)),
                new TemplateColumn<ItemViewModel>("Backup Information", new ItemHintColumnTemplate(item => item.Backup, item => item.BackupHint))
            }
        };

        this.CommandHandWritingRecognition = ReactiveCommand.CreateFromTask(this.HandWritingRecognition, this.HandWritingRecognition_CanExecute());
        this.CommandProcess = ReactiveCommand.CreateFromTask(this.Process, this.Process_CanExecute());
        this.CommandRefresh = ReactiveCommand.CreateFromTask(this.Refresh);
        this.CommandUploadTemplate = ReactiveCommand.CreateFromTask(this.UploadTemplate, this.UploadTemplate_CanExecute());

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
        ItemViewModel? selectedItem = this.TreeSource.RowSelection!.SelectedItem;
        if (selectedItem != null)
        {
            String text = await this.controller.HandWritingRecognition(selectedItem.Source, "de_DE").ConfigureAwait(true);
            await this.ShowDialog.Handle(new HandWritingRecognitionViewModel(text));
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
        try
        {
            List<ItemViewModel> items = this.TreeSource.Items.ToList();
            foreach (ItemViewModel item in items)
            {
                await this.Process(item).ConfigureAwait(true);
            }
        }
        catch
        {
            this.TreeSource.Items = new List<ItemViewModel>();
            throw;
        }
        finally
        {
            this.HasItems = this.TreeSource.Items.Any();
        }
    }

    private async Task Process(ItemViewModel item)
    {
        if (item.Collection != null)
        {
            foreach (ItemViewModel childItem in item.Collection)
            {
                await this.Process(childItem).ConfigureAwait(true);
            }
        }

        Boolean changed = await this.controller.BackupItem(item.Source).ConfigureAwait(true);

        if (this.ConnectionStatus == null)
        {
            changed |= await this.controller.SyncItem(item.Source).ConfigureAwait(true);
        }

        if (changed) { item.RaiseChanged(); }
    }

    private IObservable<Boolean> Process_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => status is null or (not TabletConnectionError.Unknown and not TabletConnectionError.SshNotConfigured and not TabletConnectionError.SshNotConnected));
        IObservable<Boolean> items = this.WhenAnyValue(vm => vm.HasItems);

        return Observable.CombineLatest(connectionStatus, items, (value1, value2) => value1 && value2);
    }

    private async Task Refresh()
    {
        try
        {
            IEnumerable<Item> items = await this.controller.GetItems().ConfigureAwait(true);

            List<ItemViewModel> list = items.Where(item => !item.Trashed).Select(item => new ItemViewModel(item, null)).ToList();
            list.Sort(new Comparison<ItemViewModel>(ItemViewModel.Compare));

            this.TreeSource.Items = list;
        }
        catch
        {
            this.TreeSource.Items = new List<ItemViewModel>();
            throw;
        }
        finally
        {
            this.HasItems = this.TreeSource.Items.Any();
        }
    }

    private async Task UpdateConnectionStatus()
    {
        while (true)
        {
            this.ConnectionStatus = await this.controller.GetConnectionStatus().ConfigureAwait(true);
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(true);
        }
    }

    private async Task UploadTemplate()
    {
        TabletTemplate tabletTemplate;

        tabletTemplate = new TabletTemplate("Lines", "Daniel", "\uE9A8", false, "/home/daniel/SynologyDrive/Remarkable/Templates/Daniel Lines.svg");
        await this.controller.UploadTemplate(tabletTemplate).ConfigureAwait(true);

        tabletTemplate = new TabletTemplate("Alignment", "Daniel", "\uEA00", false, "/home/daniel/SynologyDrive/Remarkable/Templates/Daniel Alignment.svg");
        await this.controller.UploadTemplate(tabletTemplate).ConfigureAwait(true);

        tabletTemplate = new TabletTemplate("Aufrichtung", "Daniel", "\uEA00", false, "/home/daniel/SynologyDrive/Remarkable/Templates/Daniel Aufrichtung.svg");
        await this.controller.UploadTemplate(tabletTemplate).ConfigureAwait(true);

        tabletTemplate = new TabletTemplate("Chakras", "Daniel", "\uE98F", false, "/home/daniel/SynologyDrive/Remarkable/Templates/Daniel Chakras.svg");
        await this.controller.UploadTemplate(tabletTemplate).ConfigureAwait(true);
    }

    private IObservable<Boolean> UploadTemplate_CanExecute()
    {
        return this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => status is null or (not TabletConnectionError.Unknown and not TabletConnectionError.SshNotConfigured and not TabletConnectionError.SshNotConnected));
    }

    public ICommand CommandHandWritingRecognition { get; }
    public ICommand CommandProcess { get; }
    public ICommand CommandRefresh { get; }
    public ICommand CommandUploadTemplate { get; }

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
                default: return "Unknown connection error";
            }
        }
    }

    public Boolean HasItems
    {
        get { return this.hasItems; }
        private set { this.RaiseAndSetIfChanged(ref this.hasItems, value); }
    }

    public Interaction<DialogWindowModel, Boolean> ShowDialog { get; }

    public HierarchicalTreeDataGridSource<ItemViewModel> TreeSource { get; }
}
