using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using ReactiveUI;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class MainWindowModel : ViewModelBase, IDisposable
{
    private static readonly FilePickerFileType FileTypeEpub = new FilePickerFileType("EPUB e-book")
    {
        Patterns = new String[1] { "*.epub" },
        AppleUniformTypeIdentifiers = new String[1] { "org.idpf.epub-container" },
        MimeTypes = new String[1] { "application/epub+zip" }
    };
    private static readonly FilePickerFileType FileTypePdf = FilePickerFileTypes.Pdf;

    private TabletConnectionError? connectionStatus;
    private readonly Controller controller;
    private Boolean hasBackupDirectory;
    private Boolean hasItems;
    private Job.Description jobs;
    private MyScriptLanguageViewModel myScriptLanguage;

    public MainWindowModel(String dataSource)
    {
        this.ItemsTree = new ItemsTreeViewModel();
        this.MyScriptLanguages = MyScriptLanguageViewModel.GetLanguages();
        this.OpenFilePicker = new Interaction<FilePickerOpenOptions, IEnumerable<String>?>();
        this.OpenFolderPicker = new Interaction<String, String?>();
        this.ShowDialog = new Interaction<DialogWindowModel, Boolean>();

        this.connectionStatus = TabletConnectionError.SshNotConnected;
        this.controller = new Controller(dataSource);
        this.hasBackupDirectory = Path.Exists(this.controller.Settings.Backup);
        this.hasItems = false;
        this.jobs = Job.Description.None;
        this.myScriptLanguage = this.MyScriptLanguages.Single(language => String.CompareOrdinal(language.Code, this.controller.Settings.MyScriptLanguage) == 0);

        this.CommandBackup = ReactiveCommand.CreateFromTask(this.Backup, this.Backup_CanExecute());
        this.CommandHandWritingRecognition = ReactiveCommand.CreateFromTask(this.HandWritingRecognition, this.HandWritingRecognition_CanExecute());
        this.CommandInstallLamyEraser = ReactiveCommand.CreateFromTask(this.InstallLamyEraser, this.InstallLamyEraser_CanExecute());
        this.CommandManageTemplates = ReactiveCommand.CreateFromTask(this.ManageTemplates, this.ManageTemplates_CanExecute());
        this.CommandSettings = ReactiveCommand.CreateFromTask(this.Settings, this.Settings_CanExecute());
        this.CommandSync = ReactiveCommand.CreateFromTask(this.Sync, this.Sync_CanExecute());
        this.CommandSyncTargetDirectory = ReactiveCommand.CreateFromTask<String>(this.SyncTargetDirectory, this.SyncTargetDirectory_CanExecute());
        this.CommandUploadFile = ReactiveCommand.CreateFromTask(this.UploadFile, this.UploadFile_CanExecute());
        this.CommandUploadTemplate = ReactiveCommand.CreateFromTask(this.UploadTemplate, this.UploadTemplate_CanExecute());

        this.WhenAnyValue(vm => vm.ConnectionStatus).Subscribe(status => this.RaisePropertyChanged(nameof(this.ConnectionStatusText)));
        this.WhenAnyValue(vm => vm.Jobs).Subscribe(jobs => this.RaisePropertyChanged(nameof(this.JobsText)));
        this.WhenAnyValue(vm => vm.MyScriptLanguage).Subscribe(this.SaveMyScriptLanguage);

        RxApp.MainThreadScheduler.Schedule(this.Update);
    }

    private async Task Backup()
    {
        using Job job = new Job(Job.Description.Backup, this);

        List<ItemViewModel> items = this.ItemsTree.Items.ToList();
        foreach (ItemViewModel item in items)
        {
            await this.Backup(item).ConfigureAwait(true);
        }
    }

    private async Task Backup(ItemViewModel item)
    {
        if (item.Collection != null)
        {
            foreach (ItemViewModel childItem in item.Collection)
            {
                await this.Backup(childItem).ConfigureAwait(true);
            }
        }

        Boolean changed = await item.Source.Backup().ConfigureAwait(true);
        if (changed) { item.RaiseChanged(ItemViewModel.RaiseChangedAdditional.Parent); }
    }

    private IObservable<Boolean> Backup_CanExecute()
    {
        IObservable<Boolean> backupDirectory = this.WhenAnyValue(vm => vm.HasBackupDirectory);
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => CheckConnectionStatusForJob(status, Job.Description.Backup));
        IObservable<Boolean> items = this.WhenAnyValue(vm => vm.HasItems);
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None or Job.Description.HandWritingRecognition);

        return Observable.CombineLatest(backupDirectory, connectionStatus, items, jobs, (value1, value2, value3, value4) => value1 && value2 && value3 && value4);
    }

    private static Boolean CheckConnectionStatusForJob(TabletConnectionError? status, Job.Description job)
    {
        switch (job)
        {
            case Job.Description.None:
            case Job.Description.SetSyncTargetDirectory:
            case Job.Description.Settings:
                return true;

            case Job.Description.Backup:
            case Job.Description.HandWritingRecognition:
            case Job.Description.UploadTemplate:
            case Job.Description.ManageTemplates:
            case Job.Description.InstallLamyEraser:
                return status is null or (not TabletConnectionError.Unknown and not TabletConnectionError.SshNotConfigured and not TabletConnectionError.SshNotConnected);

            case Job.Description.Sync:
            case Job.Description.Upload:
                return status is null;

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

    private async Task InstallLamyEraser()
    {
        using Job job = new Job(Job.Description.InstallLamyEraser, this);

        LamyEraserOptionsViewModel options = new LamyEraserOptionsViewModel();
        if (await this.ShowDialog.Handle(options))
        {
            await this.controller.InstallLamyEraser(options.Press != 0, options.Undo != 0, options.LeftHanded != 0).ConfigureAwait(true);
        }
    }

    private IObservable<Boolean> InstallLamyEraser_CanExecute()
    {
        return this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => CheckConnectionStatusForJob(status, Job.Description.InstallLamyEraser));
    }

    private async Task ManageTemplates()
    {
        using Job job = new Job(Job.Description.ManageTemplates, this);

        TemplatesViewModel templates = new TemplatesViewModel(this.controller.GetTemplates());
        if (templates.Templates.Any())
        {
            Boolean restartRequired = false;
            if (await this.ShowDialog.Handle(templates))
            {
                await Task.WhenAll(templates.Templates.Select(template => template.Restore())).ConfigureAwait(true);
                restartRequired = templates.Templates.Any();
            }

            if (restartRequired || templates.RestartRequired)
            {
                await this.Restart().ConfigureAwait(true);
            }
        }
        else
        {
            job.Done();

            await this.UploadTemplate().ConfigureAwait(true);
        }
    }

    private IObservable<Boolean> ManageTemplates_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => CheckConnectionStatusForJob(status, Job.Description.ManageTemplates));
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None);

        return Observable.CombineLatest(connectionStatus, jobs, (value1, value2) => value1 && value2);
    }

    private async Task Restart()
    {
        MessageViewModel message = MessageViewModel.Question("Restart",
@"The template information has been changed. A restart is required for the changes to take effect.
Please save your work on your tablet by going to the main screen before restarting.

Would you like to restart your reMarkable tablet now?");

        if (await this.ShowDialog.Handle(message))
        {
            await this.controller.Restart().ConfigureAwait(true);
        }
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

        this.HasBackupDirectory = Path.Exists(this.controller.Settings.Backup);
    }

    private IObservable<Boolean> Settings_CanExecute()
    {
        return this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None or Job.Description.HandWritingRecognition);
    }

    private async Task Sync()
    {
        using Job job = new Job(Job.Description.Sync, this);

        List<ItemViewModel> items = this.ItemsTree.Items.ToList();
        foreach (ItemViewModel item in items)
        {
            await this.Sync(item).ConfigureAwait(true);
        }
    }

    private async Task Sync(ItemViewModel item)
    {
        if (item.Collection != null)
        {
            foreach (ItemViewModel childItem in item.Collection)
            {
                await this.Sync(childItem).ConfigureAwait(true);
            }
        }

        Boolean changed = await item.Source.Sync().ConfigureAwait(true);
        if (changed) { item.RaiseChanged(ItemViewModel.RaiseChangedAdditional.Parent); }
    }

    private IObservable<Boolean> Sync_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => CheckConnectionStatusForJob(status, Job.Description.Sync));
        IObservable<Boolean> items = this.WhenAnyValue(vm => vm.HasItems);
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None or Job.Description.HandWritingRecognition);

        return Observable.CombineLatest(connectionStatus, items, jobs, (value1, value2, value3) => value1 && value2 && value3);
    }

    private async Task SyncTargetDirectory(String setString)
    {
        ItemViewModel? selectedItem = this.ItemsTree.RowSelection!.SelectedItem;
        if (selectedItem != null)
        {
            using Job job = new Job(Job.Description.SetSyncTargetDirectory, this);

            if (Boolean.TryParse(setString, out Boolean set) && set)
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

    private async void Update()
    {
        while (true)
        {
            this.ConnectionStatus = await this.controller.GetConnectionStatus().ConfigureAwait(true);

            Boolean updated = await this.UpdateItems().ConfigureAwait(true);

            await Task.Delay(TimeSpan.FromSeconds(updated ? 10 : 1)).ConfigureAwait(true);
        }
    }

    private async Task<Boolean> UpdateItems()
    {
        try
        {
            if (this.ConnectionStatus is null or (not TabletConnectionError.Unknown and not TabletConnectionError.SshNotConfigured and not TabletConnectionError.SshNotConnected))
            {
                if (this.Jobs is Job.Description.None or Job.Description.HandWritingRecognition)
                {
                    IEnumerable<Item> items = await this.controller.GetItems().ConfigureAwait(true);

                    List<ItemViewModel> list = items.Where(item => !item.Trashed).Select(item => new ItemViewModel(item, null)).ToList();
                    list.Sort(new Comparison<ItemViewModel>(ItemViewModel.Compare));

                    this.ItemsTree.Items = list;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                this.ItemsTree.Items = new List<ItemViewModel>();
                return false;
            }
        }
        catch (TabletException)
        {
            this.ItemsTree.Items = new List<ItemViewModel>();
            return false;
        }
        finally
        {
            this.HasItems = this.ItemsTree.Items.Any();
        }
    }

    private async Task UploadFile()
    {
        using Job job = new Job(Job.Description.Upload, this);

        FilePickerOpenOptions options = new FilePickerOpenOptions() { AllowMultiple = false, Title = "File", FileTypeFilter = new[] { FileTypePdf, FileTypeEpub } };
        IEnumerable<String>? files = await this.OpenFilePicker.Handle(options);
        String? file = files?.SingleOrDefault();
        if (file != null)
        {
            ItemViewModel? parentItem = this.ItemsTree.RowSelection!.SelectedItem;
            await this.controller.UploadFile(file, parentItem?.Source).ConfigureAwait(true);
        }
    }

    private IObservable<Boolean> UploadFile_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => CheckConnectionStatusForJob(status, Job.Description.Upload));
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None);

        return Observable.CombineLatest(connectionStatus, jobs, (value1, value2) => value1 && value2);
    }

    private async Task UploadTemplate()
    {
        using Job job = new Job(Job.Description.UploadTemplate, this);

        TemplateUploadViewModel template = new TemplateUploadViewModel();
        if (await this.ShowDialog.Handle(template))
        {
            TabletTemplate tabletTemplate = new TabletTemplate(this.controller, template.Name, template.Category, template.Icon.Code, template.SourceFilePath);
            await tabletTemplate.Upload().ConfigureAwait(true);
            await this.Restart().ConfigureAwait(true);
        }
    }

    private IObservable<Boolean> UploadTemplate_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => CheckConnectionStatusForJob(status, Job.Description.UploadTemplate));
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None);

        return Observable.CombineLatest(connectionStatus, jobs, (value1, value2) => value1 && value2);
    }

    public ReactiveCommand<Unit, Unit> CommandBackup { get; }

    public ReactiveCommand<Unit, Unit> CommandHandWritingRecognition { get; }

    public ReactiveCommand<Unit, Unit> CommandInstallLamyEraser { get; }

    public ReactiveCommand<Unit, Unit> CommandManageTemplates { get; }

    public ReactiveCommand<Unit, Unit> CommandSettings { get; }

    public ReactiveCommand<Unit, Unit> CommandSync { get; }

    public ReactiveCommand<String, Unit> CommandSyncTargetDirectory { get; }

    public ReactiveCommand<Unit, Unit> CommandUploadFile { get; }

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

    private Boolean HasBackupDirectory
    {
        get { return this.hasBackupDirectory; }
        set { this.RaiseAndSetIfChanged(ref this.hasBackupDirectory, value); }
    }

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

            if (this.Jobs.HasFlag(Job.Description.Sync)) { jobs.Add("Syncing"); }
            if (this.Jobs.HasFlag(Job.Description.Backup)) { jobs.Add("Backup"); }
            if (this.Jobs.HasFlag(Job.Description.HandWritingRecognition)) { jobs.Add("Hand Writing Recognition"); }
            if (this.Jobs.HasFlag(Job.Description.Upload)) { jobs.Add("Uploading File"); }
            if (this.Jobs.HasFlag(Job.Description.UploadTemplate)) { jobs.Add("Uploading Template"); }
            if (this.Jobs.HasFlag(Job.Description.ManageTemplates)) { jobs.Add("Managing Templates"); }
            if (this.Jobs.HasFlag(Job.Description.InstallLamyEraser)) { jobs.Add("Installing Lamy Eraser"); }

            return (jobs.Count > 0) ? String.Join(" and ", jobs) : null;
        }
    }

    public MyScriptLanguageViewModel MyScriptLanguage
    {
        get { return this.myScriptLanguage; }
        set { this.RaiseAndSetIfChanged(ref this.myScriptLanguage, value); }
    }

    public IEnumerable<MyScriptLanguageViewModel> MyScriptLanguages { get; }

    public Interaction<FilePickerOpenOptions, IEnumerable<String>?> OpenFilePicker { get; }

    public Interaction<String, String?> OpenFolderPicker { get; }

    public Interaction<DialogWindowModel, Boolean> ShowDialog { get; }

    private sealed class Job : IDisposable
    {
        [Flags]
        public enum Description
        {
            None = 0,
            Sync = 1,
            Backup = 2,
            HandWritingRecognition = 4,
            Upload = 8,
            UploadTemplate = 16,
            ManageTemplates = 32,
            SetSyncTargetDirectory = 64,
            InstallLamyEraser = 128,
            Settings = 256
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
