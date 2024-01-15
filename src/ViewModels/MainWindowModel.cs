using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class MainWindowModel : ViewModelBase, IDisposable
{
    private TabletConnectionError? connectionStatus;
    private readonly Controller controller;
    private Boolean hasItems;
    private Job.Description jobs;
    private MyScriptLanguageViewModel myScriptLanguage;

    public MainWindowModel(String dataSource)
    {
        this.ItemsTree = new ItemsTreeViewModel();
        this.MyScriptLanguages = MyScriptLanguageViewModel.GetLanguages();
        this.OpenFolderPicker = new Interaction<String, String?>();
        this.ShowDialog = new Interaction<DialogWindowModel, Boolean>();

        this.connectionStatus = TabletConnectionError.SshNotConnected;
        this.controller = new Controller(dataSource);
        this.hasItems = false;
        this.jobs = Job.Description.None;
        this.myScriptLanguage = this.MyScriptLanguages.Single(language => String.CompareOrdinal(language.Code, this.controller.Settings.MyScriptLanguage) == 0);

        this.CommandHandWritingRecognition = ReactiveCommand.CreateFromTask(this.HandWritingRecognition, this.HandWritingRecognition_CanExecute());
        this.CommandProcess = ReactiveCommand.CreateFromTask(this.Process, this.Process_CanExecute());
        this.CommandRefresh = ReactiveCommand.CreateFromTask(this.Refresh, this.Refresh_CanExecute());
        this.CommandRestoreTemplates = ReactiveCommand.CreateFromTask(this.RestoreTemplates, this.RestoreTemplates_CanExecute());
        this.CommandSettings = ReactiveCommand.CreateFromTask(this.Settings, this.Settings_CanExecute());
        this.CommandSyncTargetDirectory = ReactiveCommand.CreateFromTask<String>(this.SyncTargetDirectory, this.SyncTargetDirectory_CanExecute());
        this.CommandUploadTemplate = ReactiveCommand.CreateFromTask(this.UploadTemplate, this.UploadTemplate_CanExecute());

        this.WhenAnyValue(vm => vm.ConnectionStatus).Subscribe(status => this.RaisePropertyChanged(nameof(this.ConnectionStatusText)));
        this.WhenAnyValue(vm => vm.Jobs).Subscribe(jobs => this.RaisePropertyChanged(nameof(this.JobsText)));
        this.WhenAnyValue(vm => vm.MyScriptLanguage).Subscribe(this.SaveMyScriptLanguage);

        _ = this.UpdateConnectionStatus();
    }

    private static Boolean CheckConnectionStatusForJob(TabletConnectionError? status, Job.Description job)
    {
        switch (job)
        {
            case Job.Description.None:
            case Job.Description.Refresh:
            case Job.Description.SetSyncTargetDirectory:
            case Job.Description.Settings:
                return true;

            case Job.Description.Process:
            case Job.Description.HandWritingRecognition:
            case Job.Description.UploadTemplate:
            case Job.Description.RestoreTemplates:
                return status is null or (not TabletConnectionError.Unknown and not TabletConnectionError.SshNotConfigured and not TabletConnectionError.SshNotConnected);

            default:
                throw new NotImplementedException();
        }
    }

    public void Dispose()
    {
        this.controller.Dispose();

        GC.SuppressFinalize(this);
    }

    private async Task HandWritingRecognition()
    {
        ItemViewModel? selectedItem = this.ItemsTree.RowSelection!.SelectedItem;
        if (selectedItem != null)
        {
            using Job job = new Job(Job.Description.HandWritingRecognition, this);

            String text = await selectedItem.Source.HandWritingRecognition().ConfigureAwait(true);

            job.Done();

            await this.ShowDialog.Handle(new HandWritingRecognitionViewModel(text));
        }
    }

    private IObservable<Boolean> HandWritingRecognition_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => CheckConnectionStatusForJob(status, Job.Description.HandWritingRecognition));
        IObservable<Boolean> treeSelection = this.ItemsTree.RowSelection!.WhenAnyValue(selection => selection.SelectedItem).Select(item => item != null && item.Collection == null);

        return Observable.CombineLatest(connectionStatus, treeSelection, (value1, value2) => value1 && value2);
    }

    private async Task Process()
    {
        try
        {
            using Job job = new Job(Job.Description.Process, this);

            List<ItemViewModel> items = this.ItemsTree.Items.ToList();
            foreach (ItemViewModel item in items)
            {
                await this.Process(item).ConfigureAwait(true);
            }
        }
        catch
        {
            this.ItemsTree.Items = new List<ItemViewModel>();
            throw;
        }
        finally
        {
            this.HasItems = this.ItemsTree.Items.Any();
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

        Boolean changed = false;

        if (!String.IsNullOrEmpty(this.controller.Settings.Backup))
        {
            changed |= await item.Source.Backup().ConfigureAwait(true);
        }

        if (this.ConnectionStatus == null)
        {
            changed |= await item.Source.Sync().ConfigureAwait(true);
        }

        if (changed) { item.RaiseChanged(ItemViewModel.RaiseChangedAdditional.Parent); }
    }

    private IObservable<Boolean> Process_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => CheckConnectionStatusForJob(status, Job.Description.Process));
        IObservable<Boolean> items = this.WhenAnyValue(vm => vm.HasItems);
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None or Job.Description.HandWritingRecognition);

        return Observable.CombineLatest(connectionStatus, items, jobs, (value1, value2, value3) => value1 && value2 && value3);
    }

    private async Task Refresh()
    {
        try
        {
            using Job job = new Job(Job.Description.Refresh, this);

            IEnumerable<Item> items = await this.controller.GetItems().ConfigureAwait(true);

            List<ItemViewModel> list = items.Where(item => !item.Trashed).Select(item => new ItemViewModel(item, null)).ToList();
            list.Sort(new Comparison<ItemViewModel>(ItemViewModel.Compare));

            this.ItemsTree.Items = list;
        }
        catch
        {
            this.ItemsTree.Items = new List<ItemViewModel>();
            throw;
        }
        finally
        {
            this.HasItems = this.ItemsTree.Items.Any();
        }
    }

    private IObservable<Boolean> Refresh_CanExecute()
    {
        return this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None or Job.Description.HandWritingRecognition);
    }

    private async Task RestoreTemplates()
    {
        using Job job = new Job(Job.Description.RestoreTemplates, this);

        await this.controller.RestoreTemplates().ConfigureAwait(true);
    }

    private IObservable<Boolean> RestoreTemplates_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => CheckConnectionStatusForJob(status, Job.Description.RestoreTemplates));
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None);

        return Observable.CombineLatest(connectionStatus, jobs, (value1, value2) => value1 && value2);
    }

    private void SaveMyScriptLanguage(MyScriptLanguageViewModel language)
    {
        this.controller.Settings.MyScriptLanguage = language.Code;
        this.controller.Settings.SaveChanges();
    }

    private async Task Settings()
    {
        using Job job = new Job(Job.Description.Settings, this);

        SettingsViewModel settings = new SettingsViewModel(this.controller.Settings);
        if (await this.ShowDialog.Handle(settings))
        {
            settings.SaveChanges();
        }
    }

    private IObservable<Boolean> Settings_CanExecute()
    {
        return this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None or Job.Description.HandWritingRecognition);
    }

    private async Task SyncTargetDirectory(String setString)
    {
        ItemViewModel? selectedItem = this.ItemsTree.RowSelection!.SelectedItem;
        if (selectedItem != null)
        {
            using Job job = new Job(Job.Description.SetSyncTargetDirectory, this);

            Boolean set;
            if (Boolean.TryParse(setString, out set) && set)
            {
                String? targetDirectory = await this.OpenFolderPicker.Handle("Sync Target Folder");
                if (targetDirectory != null)
                {
                    selectedItem.Source.SetSyncTargetDirectory(targetDirectory);
                    selectedItem.RaiseChanged(ItemViewModel.RaiseChangedAdditional.Collection);
                }
            }
            else
            {
                selectedItem.Source.SetSyncTargetDirectory(null);
                selectedItem.RaiseChanged(ItemViewModel.RaiseChangedAdditional.Collection);
            }
        }
    }

    private IObservable<Boolean> SyncTargetDirectory_CanExecute()
    {
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None or Job.Description.HandWritingRecognition);
        IObservable<Boolean> treeSelection = this.ItemsTree.RowSelection!.WhenAnyValue(selection => selection.SelectedItem).Select(item => item != null);

        return Observable.CombineLatest(jobs, treeSelection, (value1, value2) => value1 && value2);
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
        using Job job = new Job(Job.Description.UploadTemplate, this);

        TemplateViewModel template = new TemplateViewModel();
        if (await this.ShowDialog.Handle(template))
        {
            TabletTemplate tabletTemplate = new TabletTemplate(template.Name, template.Category, template.Icon.Code, template.SourceFilePath);
            await this.controller.UploadTemplate(tabletTemplate).ConfigureAwait(true);
        }
    }

    private IObservable<Boolean> UploadTemplate_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => CheckConnectionStatusForJob(status, Job.Description.UploadTemplate));
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None);

        return Observable.CombineLatest(connectionStatus, jobs, (value1, value2) => value1 && value2);
    }

    public ReactiveCommand<Unit, Unit> CommandHandWritingRecognition { get; }

    public ReactiveCommand<Unit, Unit> CommandProcess { get; }

    public ReactiveCommand<Unit, Unit> CommandRefresh { get; }

    public ReactiveCommand<Unit, Unit> CommandRestoreTemplates { get; }

    public ReactiveCommand<Unit, Unit> CommandSettings { get; }

    public ReactiveCommand<String, Unit> CommandSyncTargetDirectory { get; }

    public ReactiveCommand<Unit, Unit> CommandUploadTemplate { get; }

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

    public ItemsTreeViewModel ItemsTree { get; }

    public Boolean HasItems
    {
        get { return this.hasItems; }
        private set { this.RaiseAndSetIfChanged(ref this.hasItems, value); }
    }

    private Job.Description Jobs
    {
        get { return this.jobs; }
        set { this.RaiseAndSetIfChanged(ref this.jobs, value); }
    }

    public String? JobsText
    {
        get
        {
            List<String> jobs = new List<String>();

            if (this.Jobs.HasFlag(Job.Description.Refresh)) { jobs.Add("Refreshing"); }
            if (this.Jobs.HasFlag(Job.Description.Process)) { jobs.Add("Backup & Syncing"); }
            if (this.Jobs.HasFlag(Job.Description.HandWritingRecognition)) { jobs.Add("Hand Writing Recognition"); }
            if (this.Jobs.HasFlag(Job.Description.UploadTemplate)) { jobs.Add("Uploading Template"); }
            if (this.Jobs.HasFlag(Job.Description.RestoreTemplates)) { jobs.Add("Restoring Templates"); }

            return (jobs.Count > 0) ? String.Join(" and ", jobs) : null;
        }
    }

    public MyScriptLanguageViewModel MyScriptLanguage
    {
        get { return this.myScriptLanguage; }
        set { this.RaiseAndSetIfChanged(ref this.myScriptLanguage, value); }
    }

    public IEnumerable<MyScriptLanguageViewModel> MyScriptLanguages { get; }

    public Interaction<String, String?> OpenFolderPicker { get; }

    public Interaction<DialogWindowModel, Boolean> ShowDialog { get; }

    private sealed class Job : IDisposable
    {
        [Flags]
        public enum Description
        {
            None = 0,
            Refresh = 1,
            Process = 2,
            HandWritingRecognition = 4,
            UploadTemplate = 8,
            RestoreTemplates = 16,
            SetSyncTargetDirectory = 32,
            Settings = 64
        }

        private readonly Description description;
        private Boolean done;
        private readonly MainWindowModel owner;

        public Job(Description description, MainWindowModel owner)
        {
            this.description = description;
            this.done = false;
            this.owner = owner;

            this.owner.Jobs |= this.description;
        }

        void IDisposable.Dispose()
        {
            if (!this.done)
            {
                this.done = true;
                this.owner.Jobs ^= this.description;
            }
        }

        public void Done()
        {
            IDisposable disposable = this;
            disposable.Dispose();
        }
    }
}
