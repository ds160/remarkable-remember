using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReMarkableRemember.Helper;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.DataService.Models;
using ReMarkableRemember.Services.HandWritingRecognition;
using ReMarkableRemember.Services.HandWritingRecognition.Configuration;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.Services.TabletService.Exceptions;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class MainWindowModel : ViewModelBase, IAppModel
{
    private readonly ServiceProvider services;

    private readonly IDataService dataService;
    private readonly IHandWritingRecognitionService handWritingRecognitionService;
    private readonly ITabletService tabletService;

    private TabletError? connectionStatus;
    private HandWritingRecognitionLanguageViewModel handWritingRecognitionLanguage;
    private Boolean hasBackupDirectory;
    private Boolean hasItems;
    private Job.Description jobs;

    public MainWindowModel(ServiceProvider services)
    {
        this.services = services;

        this.dataService = this.services.GetRequiredService<IDataService>();
        this.handWritingRecognitionService = this.services.GetRequiredService<IHandWritingRecognitionService>();
        this.tabletService = this.services.GetRequiredService<ITabletService>();

        this.ItemsTree = new ItemsTreeViewModel();
        this.HandWritingRecognitionLanguages = HandWritingRecognitionLanguageViewModel.GetLanguages(this.handWritingRecognitionService);
        this.OpenFilePicker = new Interaction<FilePickerOpenOptions, IEnumerable<String>?>();
        this.OpenFolderPicker = new Interaction<String, String?>();
        this.OpenSaveFilePicker = new Interaction<FilePickerSaveOptions, String?>();
        this.ShowDialog = new Interaction<DialogWindowModel, Boolean>();

        this.connectionStatus = TabletError.SshNotConnected;
        this.handWritingRecognitionLanguage = this.HandWritingRecognitionLanguages.Single(language => String.CompareOrdinal(language.Code, this.handWritingRecognitionService.Configuration.Language) == 0);
        this.hasBackupDirectory = Path.Exists(this.tabletService.Configuration.Backup);
        this.hasItems = false;
        this.jobs = Job.Description.None;

        this.CommandAbout = ReactiveCommand.CreateFromTask(this.About);
        this.CommandBackup = ReactiveCommand.CreateFromTask(() => this.Execute(Job.Description.Backup), this.Execute_CanExecute(Job.Description.Backup));
        this.CommandDownloadFile = ReactiveCommand.CreateFromTask(this.DownloadFile, this.DownloadFile_CanExecute());
        this.CommandExecute = ReactiveCommand.CreateFromTask(() => this.Execute(Job.Description.Backup | Job.Description.Sync), this.Execute_CanExecute(Job.Description.Backup | Job.Description.Sync));
        this.CommandHandwritingRecognition = ReactiveCommand.CreateFromTask(this.HandwritingRecognition, this.HandwritingRecognition_CanExecute());
        this.CommandInstallLamyEraser = ReactiveCommand.CreateFromTask(this.InstallLamyEraser, this.InstallLamyEraser_CanExecute());
        this.CommandInstallWebInterfaceOnBoot = ReactiveCommand.CreateFromTask(this.InstallWebInterfaceOnBoot, this.InstallWebInterfaceOnBoot_CanExecute());
        this.CommandManageTemplates = ReactiveCommand.CreateFromTask(this.ManageTemplates, this.ManageTemplates_CanExecute());
        this.CommandOpenItem = ReactiveCommand.Create(this.OpenItem, this.OpenItem_CanExecute());
        this.CommandSettings = ReactiveCommand.CreateFromTask(this.Settings, this.Settings_CanExecute());
        this.CommandSync = ReactiveCommand.CreateFromTask(() => this.Execute(Job.Description.Sync), this.Execute_CanExecute(Job.Description.Sync));
        this.CommandSyncTargetDirectory = ReactiveCommand.CreateFromTask<String>(this.SyncTargetDirectory, this.SyncTargetDirectory_CanExecute());
        this.CommandUploadFile = ReactiveCommand.CreateFromTask(this.UploadFile, this.UploadFile_CanExecute());
        this.CommandUploadTemplate = ReactiveCommand.CreateFromTask(this.UploadTemplate, this.UploadTemplate_CanExecute());

        this.WhenAnyValue(vm => vm.ConnectionStatus).Subscribe(status => this.RaisePropertyChanged(nameof(this.ConnectionStatusText)));
        this.WhenAnyValue(vm => vm.Jobs).Subscribe(jobs => this.RaisePropertyChanged(nameof(this.JobsText)));
        this.WhenAnyValue(vm => vm.HandWritingRecognitionLanguage).Subscribe(this.SaveHandWritingRecognitionLanguage);

        RxApp.MainThreadScheduler.Schedule(this.Update);
    }

    private async Task About()
    {
        await this.ShowDialog.Handle(new AboutViewModel());
    }

    private static Boolean CheckConnectionStatusForJob(TabletError? status, Job.Description job)
    {
        switch (job)
        {
            case Job.Description.None:
            case Job.Description.SetSyncTargetDirectory:
            case Job.Description.Settings:
                return true;

            case Job.Description.GetItems:
            case Job.Description.Backup:
            case Job.Description.HandwritingRecognition:
            case Job.Description.UploadTemplate:
            case Job.Description.ManageTemplates:
            case Job.Description.InstallLamyEraser:
            case Job.Description.InstallWebInterfaceOnBoot:
                return status is null or (not TabletError.NotSupported and not TabletError.Unknown and not TabletError.SshNotConfigured and not TabletError.SshNotConnected);

            case Job.Description.Sync:
            case Job.Description.Download:
            case Job.Description.Upload:
                return status is null;

            default:
                throw new NotImplementedException();
        }
    }

    private async Task DownloadFile()
    {
        ItemViewModel? selectedItem = this.ItemsTree.RowSelection!.SelectedItem;
        if (selectedItem != null && selectedItem.Collection == null)
        {
            using Job job = new Job(Job.Description.Download, this);

            FilePickerSaveOptions options = new FilePickerSaveOptions() { DefaultExtension = "pdf", FileTypeChoices = new[] { FilePickerFileTypes.Pdf } };
            String? targetPath = await this.OpenSaveFilePicker.Handle(options);
            if (targetPath != null)
            {
                await this.tabletService.Download(selectedItem.Id, targetPath);
            }
        }
    }

    private IObservable<Boolean> DownloadFile_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => CheckConnectionStatusForJob(status, Job.Description.Download));
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None);
        IObservable<Boolean> treeSelection = this.ItemsTree.RowSelection!.WhenAnyValue(selection => selection.SelectedItem).Select(item => item != null && item.Collection == null);

        return Observable.CombineLatest(connectionStatus, jobs, treeSelection, (value1, value2, value3) => value1 && value2 && value3);
    }

    private async Task Execute(Job.Description jobDescription)
    {
        using Job job = new Job(jobDescription, this);

        List<ItemViewModel> items = this.ItemsTree.Items.ToList();
        foreach (ItemViewModel item in items)
        {
            await this.Execute(item, jobDescription).ConfigureAwait(true);
        }
    }

    private async Task Execute(ItemViewModel item, Job.Description job)
    {
        TabletError? status = this.ConnectionStatus;

        if (item.Collection != null)
        {
            foreach (ItemViewModel childItem in item.Collection)
            {
                await this.Execute(childItem, job).ConfigureAwait(true);
            }
        }

        if (job.HasFlag(Job.Description.Backup) && CheckConnectionStatusForJob(status, Job.Description.Backup)) { await item.Backup().ConfigureAwait(true); }
        if (job.HasFlag(Job.Description.Sync) && CheckConnectionStatusForJob(status, Job.Description.Sync)) { await item.Sync().ConfigureAwait(true); }
    }

    private IObservable<Boolean> Execute_CanExecute(Job.Description jobDescription)
    {
        IObservable<Boolean> backupDirectory = this.WhenAnyValue(vm => vm.HasBackupDirectory).Select(hasBackupDirectory => jobDescription != Job.Description.Backup || hasBackupDirectory);
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => CheckConnectionStatusForJob(status, (jobDescription == Job.Description.Sync) ? Job.Description.Sync : Job.Description.Backup));
        IObservable<Boolean> items = this.WhenAnyValue(vm => vm.HasItems);
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None or Job.Description.HandwritingRecognition);

        return Observable.CombineLatest(backupDirectory, connectionStatus, items, jobs, (value1, value2, value3, value4) => value1 && value2 && value3 && value4);
    }

    private async Task HandwritingRecognition()
    {
        ItemViewModel? selectedItem = this.ItemsTree.RowSelection!.SelectedItem;
        if (selectedItem != null && selectedItem.Collection == null)
        {
            using Job job = new Job(Job.Description.HandwritingRecognition, this);

            String text = await selectedItem.HandWritingRecognition().ConfigureAwait(true);

            job.Done();

            await this.ShowDialog.Handle(new HandwritingRecognitionViewModel(text));
        }
    }

    private IObservable<Boolean> HandwritingRecognition_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => CheckConnectionStatusForJob(status, Job.Description.HandwritingRecognition));
        IObservable<Boolean> treeSelection = this.ItemsTree.RowSelection!.WhenAnyValue(selection => selection.SelectedItem).Select(item => item != null && item.Collection == null);

        return Observable.CombineLatest(connectionStatus, treeSelection, (value1, value2) => value1 && value2);
    }

    private async Task InstallLamyEraser()
    {
        using Job job = new Job(Job.Description.InstallLamyEraser, this);

        await this.ShowDialog.Handle(new LamyEraserViewModel(this.services));
    }

    private IObservable<Boolean> InstallLamyEraser_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => CheckConnectionStatusForJob(status, Job.Description.InstallLamyEraser));
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None);

        return Observable.CombineLatest(connectionStatus, jobs, (value1, value2) => value1 && value2);
    }

    private async Task InstallWebInterfaceOnBoot()
    {
        using Job job = new Job(Job.Description.InstallWebInterfaceOnBoot, this);

        await this.tabletService.InstallWebInterfaceOnBoot().ConfigureAwait(true);

        await this.Restart(job).ConfigureAwait(true);
    }

    private IObservable<Boolean> InstallWebInterfaceOnBoot_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => CheckConnectionStatusForJob(status, Job.Description.InstallWebInterfaceOnBoot));
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None);

        return Observable.CombineLatest(connectionStatus, jobs, (value1, value2) => value1 && value2);
    }

    private async Task ManageTemplates()
    {
        using Job job = new Job(Job.Description.ManageTemplates, this);

        IEnumerable<TemplateData> dataTemplates = await this.dataService.GetTemplates().ConfigureAwait(true);
        IEnumerable<TabletTemplate> tabletTemplates = dataTemplates.Select(template => new TabletTemplate(template.Name, template.Category, template.IconCode, template.BytesPng, template.BytesSvg)).ToArray();
        TemplatesViewModel templates = new TemplatesViewModel(tabletTemplates, this.services);
        if (templates.Templates.Any())
        {
            await this.ShowDialog.Handle(templates);
            if (templates.RestartRequired)
            {
                await this.Restart(job).ConfigureAwait(true);
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

    private void OpenItem()
    {
        ItemViewModel? selectedItem = this.ItemsTree.RowSelection!.SelectedItem;
        if (selectedItem?.SyncPath != null)
        {
            Process.Start(new ProcessStartInfo(selectedItem.SyncPath) { UseShellExecute = true });
        }
    }

    private IObservable<Boolean> OpenItem_CanExecute()
    {
        return this.ItemsTree.RowSelection!.WhenAnyValue(selection => selection.SelectedItem).Select(item => Path.Exists(item?.SyncPath));
    }

    private async Task Restart(Job job)
    {
        job.Done();

        String reason = String.Empty;
        if (job.HasFlag(Job.Description.ManageTemplates) || job.HasFlag(Job.Description.UploadTemplate))
        {
            reason = "The template information has been changed. ";
        }

        MessageViewModel message = MessageViewModel.Question("Restart", String.Join(Environment.NewLine,
            $"{reason}A restart is required for the changes to take effect.",
            "Please save your work on your tablet by going to the main screen before restarting.",
            "",
            "Would you like to restart your reMarkable tablet now?"));

        if (await this.ShowDialog.Handle(message))
        {
            await this.tabletService.Restart().ConfigureAwait(true);
        }
    }

    private async void SaveHandWritingRecognitionLanguage(HandWritingRecognitionLanguageViewModel language)
    {
        IHandWritingRecognitionConfiguration configuration = this.handWritingRecognitionService.Configuration;
        configuration.Language = language.Code;
        await configuration.Save().ConfigureAwait(true);
    }

    private async Task Settings()
    {
        using Job job = new Job(Job.Description.Settings, this);

        await this.ShowDialog.Handle(new SettingsViewModel(this.services));

        this.HasBackupDirectory = Path.Exists(this.tabletService.Configuration.Backup);
        this.HandWritingRecognitionLanguage = this.HandWritingRecognitionLanguages.Single(language => String.CompareOrdinal(language.Code, this.handWritingRecognitionService.Configuration.Language) == 0);
    }

    private IObservable<Boolean> Settings_CanExecute()
    {
        return this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None or Job.Description.HandwritingRecognition);
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
                    await selectedItem.SetSyncTargetDirectory(targetDirectory).ConfigureAwait(true);
                }
            }
            else
            {
                await selectedItem.SetSyncTargetDirectory(null).ConfigureAwait(true);
            }
        }
    }

    private IObservable<Boolean> SyncTargetDirectory_CanExecute()
    {
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None or Job.Description.HandwritingRecognition);
        IObservable<Boolean> treeSelection = this.ItemsTree.RowSelection!.WhenAnyValue(selection => selection.SelectedItem).Select(item => item != null);

        return Observable.CombineLatest(jobs, treeSelection, (value1, value2) => value1 && value2);
    }

    private async void Update()
    {
        while (true)
        {
            this.ConnectionStatus = await this.tabletService.GetConnectionStatus().ConfigureAwait(true);

            Boolean updated = await this.UpdateItems().ConfigureAwait(true);

            await Task.Delay(TimeSpan.FromSeconds(updated ? 10 : 1)).ConfigureAwait(true);
        }
    }

    private async Task<Boolean> UpdateItems()
    {
        try
        {
            if (CheckConnectionStatusForJob(this.ConnectionStatus, Job.Description.GetItems))
            {
                if (this.Jobs is Job.Description.None or Job.Description.HandwritingRecognition)
                {
                    using Job? job = this.HasItems ? null : new Job(Job.Description.GetItems, this);

                    IEnumerable<TabletItem> tabletItemsAll = await this.tabletService.GetItems().ConfigureAwait(true);
                    IEnumerable<TabletItem> tabletItems = tabletItemsAll.Where(item => !item.Trashed).ToArray();

                    await ItemViewModel.UpdateItems(tabletItems, this.ItemsTree.Items, null, this.services).ConfigureAwait(true);

                    this.ItemsTree.Sort(new Comparison<ItemViewModel>(ItemViewModel.Compare));

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                this.ItemsTree.Items.Clear();
                return false;
            }
        }
        catch (TabletException)
        {
            this.ItemsTree.Items.Clear();
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

        FilePickerOpenOptions options = new FilePickerOpenOptions() { AllowMultiple = true, FileTypeFilter = new[] { FilePickerFileTypes.Pdf, FilePickerFileTypesExtensions.Epub } };
        IEnumerable<String>? files = await this.OpenFilePicker.Handle(options);
        foreach (String file in files)
        {
            ItemViewModel? parentItem = this.ItemsTree.RowSelection!.SelectedItem;
            String? parentId = UploadFileParentId(parentItem);
            await this.tabletService.UploadFile(file, parentId).ConfigureAwait(true);
        }
    }

    private static String? UploadFileParentId(ItemViewModel? parentItem)
    {
        while (parentItem != null)
        {
            if (parentItem.Collection != null)
            {
                return parentItem.Id;
            }

            parentItem = parentItem.Parent;
        }

        return null;
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

        if (await this.ShowDialog.Handle(new TemplateUploadViewModel(this.services)))
        {
            await this.Restart(job).ConfigureAwait(true);
        }
    }

    private IObservable<Boolean> UploadTemplate_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => CheckConnectionStatusForJob(status, Job.Description.UploadTemplate));
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None);

        return Observable.CombineLatest(connectionStatus, jobs, (value1, value2) => value1 && value2);
    }

    public ICommand CommandAbout { get; }

    public ICommand CommandBackup { get; }

    public ICommand CommandDownloadFile { get; }

    public ICommand CommandExecute { get; }

    public ICommand CommandHandwritingRecognition { get; }

    public ICommand CommandInstallLamyEraser { get; }

    public ICommand CommandInstallWebInterfaceOnBoot { get; }

    public ICommand CommandManageTemplates { get; }

    public ICommand CommandOpenItem { get; }

    public ICommand CommandSettings { get; }

    public ICommand CommandSync { get; }

    public ReactiveCommand<String, Unit> CommandSyncTargetDirectory { get; }

    public ICommand CommandUploadFile { get; }

    public ICommand CommandUploadTemplate { get; }

    public TabletError? ConnectionStatus
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
                case TabletError.NotSupported: return "Connected reMarkable not supported";
                case TabletError.Unknown: return "Not connected";
                case TabletError.SshNotConfigured: return "SSH protocol information are not configured or wrong";
                case TabletError.SshNotConnected: return "Not connected via WiFi or USB";
                case TabletError.UsbNotActived: return "USB web interface is not activated";
                case TabletError.UsbNotConnected: return "Not connected via USB";
                default: return "Not connected";
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

            if (this.Jobs.HasFlag(Job.Description.GetItems)) { jobs.Add("Getting Items"); }
            if (this.Jobs.HasFlag(Job.Description.Sync)) { jobs.Add("Syncing"); }
            if (this.Jobs.HasFlag(Job.Description.Backup)) { jobs.Add("Backup"); }
            if (this.Jobs.HasFlag(Job.Description.HandwritingRecognition)) { jobs.Add("Handwriting Recognition"); }
            if (this.Jobs.HasFlag(Job.Description.Download)) { jobs.Add("Downloading File"); }
            if (this.Jobs.HasFlag(Job.Description.Upload)) { jobs.Add("Uploading File"); }
            if (this.Jobs.HasFlag(Job.Description.UploadTemplate)) { jobs.Add("Uploading Template"); }
            if (this.Jobs.HasFlag(Job.Description.ManageTemplates)) { jobs.Add("Managing Templates"); }
            if (this.Jobs.HasFlag(Job.Description.InstallLamyEraser)) { jobs.Add("Installing Lamy Eraser"); }
            if (this.Jobs.HasFlag(Job.Description.InstallWebInterfaceOnBoot)) { jobs.Add("Installing WebInterface-OnBoot"); }

            return (jobs.Count > 0) ? String.Join(" and ", jobs) : null;
        }
    }

    public HandWritingRecognitionLanguageViewModel HandWritingRecognitionLanguage
    {
        get { return this.handWritingRecognitionLanguage; }
        set { this.RaiseAndSetIfChanged(ref this.handWritingRecognitionLanguage, value); }
    }

    public IEnumerable<HandWritingRecognitionLanguageViewModel> HandWritingRecognitionLanguages { get; }

    public Interaction<FilePickerOpenOptions, IEnumerable<String>?> OpenFilePicker { get; }

    public Interaction<String, String?> OpenFolderPicker { get; }

    public Interaction<FilePickerSaveOptions, String?> OpenSaveFilePicker { get; }

    public Interaction<DialogWindowModel, Boolean> ShowDialog { get; }

    public static String Title
    {
        get { return $"reMarkable Remember - {Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)}"; }
    }

    private sealed class Job : IDisposable
    {
        [Flags]
        public enum Description
        {
            None = 0x0000,
            GetItems = 0x0001,
            Sync = 0x0002,
            Backup = 0x0004,
            HandwritingRecognition = 0x0008,
            Download = 0x0010,
            Upload = 0x0020,
            UploadTemplate = 0x0040,
            ManageTemplates = 0x0080,
            SetSyncTargetDirectory = 0x0100,
            InstallLamyEraser = 0x0200,
            InstallWebInterfaceOnBoot = 0x0400,
            Settings = 0x0800
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

        public Boolean HasFlag(Description flag)
        {
            return this.description.HasFlag(flag);
        }
    }
}
