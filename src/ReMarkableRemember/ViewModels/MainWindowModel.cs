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
using ReactiveUI;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class MainWindowModel : ViewModelBase, IAppModel, IDisposable
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

        this.CommandAbout = ReactiveCommand.CreateFromTask(this.About);
        this.CommandBackup = ReactiveCommand.CreateFromTask(() => this.Execute(Job.Description.Backup), this.Execute_CanExecute(Job.Description.Backup));
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
        this.WhenAnyValue(vm => vm.MyScriptLanguage).Subscribe(this.SaveMyScriptLanguage);

        RxApp.MainThreadScheduler.Schedule(this.Update);
    }

    private async Task About()
    {
        if (await this.ShowDialog.Handle(new AboutViewModel()))
        {
            Process.Start(new ProcessStartInfo("https://github.com/ds160/remarkable-remember") { UseShellExecute = true });
        }
    }

    private static Boolean CheckConnectionStatusForJob(TabletConnectionError? status, Job.Description job)
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
                return status is null or (not TabletConnectionError.NotSupported and not TabletConnectionError.Unknown and not TabletConnectionError.SshNotConfigured and not TabletConnectionError.SshNotConnected);

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
        TabletConnectionError? status = this.ConnectionStatus;

        if (item.Collection != null)
        {
            foreach (ItemViewModel childItem in item.Collection)
            {
                await this.Execute(childItem, job).ConfigureAwait(true);
            }
        }

        Boolean changed = false;
        if (job.HasFlag(Job.Description.Backup) && CheckConnectionStatusForJob(status, Job.Description.Backup)) { changed |= await item.Source.Backup().ConfigureAwait(true); }
        if (job.HasFlag(Job.Description.Sync) && CheckConnectionStatusForJob(status, Job.Description.Sync)) { changed |= await item.Source.Sync().ConfigureAwait(true); }
        if (changed) { item.RaiseChanged(ItemViewModel.RaiseChangedAdditional.Parent); }
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
        if (selectedItem != null)
        {
            using Job job = new Job(Job.Description.HandwritingRecognition, this);

            String text = await selectedItem.Source.HandwritingRecognition().ConfigureAwait(true);

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

        LamyEraserOptionsViewModel options = new LamyEraserOptionsViewModel();
        if (await this.ShowDialog.Handle(options))
        {
            await this.controller.InstallLamyEraser(options.Press != 0, options.Undo != 0, options.LeftHanded != 0).ConfigureAwait(true);
        }
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

        await this.controller.InstallWebInterfaceOnBoot().ConfigureAwait(true);

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
        this.MyScriptLanguage = this.MyScriptLanguages.Single(language => String.CompareOrdinal(language.Code, this.controller.Settings.MyScriptLanguage) == 0);
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
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Job.Description.None or Job.Description.HandwritingRecognition);
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
            if (CheckConnectionStatusForJob(this.ConnectionStatus, Job.Description.GetItems))
            {
                if (this.Jobs is Job.Description.None or Job.Description.HandwritingRecognition)
                {
                    using Job? job = this.HasItems ? null : new Job(Job.Description.GetItems, this);

                    IEnumerable<Item> items = await this.controller.GetItems().ConfigureAwait(true);
                    ItemViewModel.UpdateItems(items, this.ItemsTree.Items, null);

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

        FilePickerOpenOptions options = new FilePickerOpenOptions() { AllowMultiple = true, FileTypeFilter = new[] { FileTypePdf, FileTypeEpub } };
        IEnumerable<String>? files = await this.OpenFilePicker.Handle(options);
        foreach (String file in files)
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
                case TabletConnectionError.NotSupported: return "Connected reMarkable not supported";
                case TabletConnectionError.Unknown: return "Not connected";
                case TabletConnectionError.SshNotConfigured: return "SSH protocol information are not configured or wrong";
                case TabletConnectionError.SshNotConnected: return "Not connected via WiFi or USB";
                case TabletConnectionError.UsbNotActived: return "USB web interface is not activated";
                case TabletConnectionError.UsbNotConnected: return "Not connected via USB";
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
            if (this.Jobs.HasFlag(Job.Description.Upload)) { jobs.Add("Uploading File"); }
            if (this.Jobs.HasFlag(Job.Description.UploadTemplate)) { jobs.Add("Uploading Template"); }
            if (this.Jobs.HasFlag(Job.Description.ManageTemplates)) { jobs.Add("Managing Templates"); }
            if (this.Jobs.HasFlag(Job.Description.InstallLamyEraser)) { jobs.Add("Installing Lamy Eraser"); }
            if (this.Jobs.HasFlag(Job.Description.InstallWebInterfaceOnBoot)) { jobs.Add("Installing WebInterface-OnBoot"); }

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
            Upload = 0x0010,
            UploadTemplate = 0x0020,
            ManageTemplates = 0x0040,
            SetSyncTargetDirectory = 0x0080,
            InstallLamyEraser = 0x0100,
            InstallWebInterfaceOnBoot = 0x0200,
            Settings = 0x0400
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
