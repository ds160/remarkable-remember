using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using DynamicData.Binding;
using ReactiveUI;
using ReMarkableRemember.Enumerations;
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
    private readonly IDataService dataService;
    private readonly IHandWritingRecognitionService handWritingRecognitionService;
    private readonly ITabletService tabletService;

    private ConnectionStatusViewModel connectionStatus;
    private HandWritingRecognitionLanguageViewModel handWritingRecognitionLanguage;
    private Boolean hasBackupDirectory;
    private Boolean hasItems;
    private Jobs jobs;

    public MainWindowModel(IDataService dataService, IHandWritingRecognitionService handWritingRecognitionService, ITabletService tabletService)
    {
        this.dataService = dataService;
        this.handWritingRecognitionService = handWritingRecognitionService;
        this.tabletService = tabletService;

        this.ItemsTree = new ItemsTreeViewModel();
        this.HandWritingRecognitionLanguages = HandWritingRecognitionLanguageViewModel.GetLanguages(this.handWritingRecognitionService);
        this.OpenFilePicker = new Interaction<FilePickerOpenOptions, IEnumerable<String>?>();
        this.OpenFolderPicker = new Interaction<String, String?>();
        this.OpenSaveFilePicker = new Interaction<FilePickerSaveOptions, String?>();
        this.ShowDialog = new Interaction<DialogWindowModel, Boolean>();

        this.connectionStatus = new ConnectionStatusViewModel();
        this.handWritingRecognitionLanguage = this.HandWritingRecognitionLanguages.Single(language => String.Equals(language.Code, this.handWritingRecognitionService.Configuration.Language, StringComparison.Ordinal));
        this.hasBackupDirectory = Path.Exists(this.tabletService.Configuration.Backup);
        this.hasItems = false;
        this.jobs = Jobs.None;

        this.CommandAbout = ReactiveCommand.CreateFromTask(this.About);
        this.CommandBackup = ReactiveCommand.CreateFromTask(() => this.Execute(Jobs.Backup), this.Execute_CanExecute(Jobs.Backup));
        this.CommandDownloadFile = ReactiveCommand.CreateFromTask(this.DownloadFile, this.DownloadFile_CanExecute());
        this.CommandExecute = ReactiveCommand.CreateFromTask(() => this.Execute(Jobs.Backup | Jobs.Sync), this.Execute_CanExecute(Jobs.Backup | Jobs.Sync));
        this.CommandHandwritingRecognition = ReactiveCommand.CreateFromTask(this.HandwritingRecognition, this.HandwritingRecognition_CanExecute());
        this.CommandInstallLamyEraser = ReactiveCommand.CreateFromTask(this.InstallLamyEraser, this.InstallLamyEraser_CanExecute());
        this.CommandManageTemplates = ReactiveCommand.CreateFromTask(this.ManageTemplates, this.ManageTemplates_CanExecute());
        this.CommandOpenItem = ReactiveCommand.Create(this.OpenItem, this.OpenItem_CanExecute());
        this.CommandSettings = ReactiveCommand.CreateFromTask(this.Settings, this.Settings_CanExecute());
        this.CommandSync = ReactiveCommand.CreateFromTask(() => this.Execute(Jobs.Sync), this.Execute_CanExecute(Jobs.Sync));
        this.CommandSyncTargetDirectory = ReactiveCommand.CreateFromTask<String>(this.SyncTargetDirectory, this.SyncTargetDirectory_CanExecute());
        this.CommandUploadFile = ReactiveCommand.CreateFromTask(this.UploadFile, this.UploadFile_CanExecute());
        this.CommandUploadTemplate = ReactiveCommand.CreateFromTask(this.UploadTemplate, this.UploadTemplate_CanExecute());

        this.WhenAnyValue(vm => vm.Jobs).Subscribe(jobs => this.RaisePropertyChanged(nameof(this.JobsText)));
        this.WhenAnyValue(vm => vm.HandWritingRecognitionLanguage).Subscribe(this.SaveHandWritingRecognitionLanguage);

        RxApp.MainThreadScheduler.Schedule(this.Update);
    }

    private async Task About()
    {
        await this.ShowDialog.Handle(new AboutViewModel());
    }

    private async Task DownloadFile()
    {
        ItemViewModel? selectedItem = this.ItemsTree.RowSelection.SelectedItem;
        if (selectedItem != null && selectedItem.Collection == null)
        {
            using Job job = new Job(Jobs.Download, this);

            FilePickerSaveOptions options = new FilePickerSaveOptions()
            {
                DefaultExtension = "pdf",
                FileTypeChoices = new[] { FilePickerFileTypes.Pdf },
                SuggestedFileName = selectedItem.Name
            };
            String? targetPath = await this.OpenSaveFilePicker.Handle(options);
            if (targetPath != null)
            {
                await this.tabletService.Download(selectedItem.Id, targetPath);
            }
        }
    }

    private IObservable<Boolean> DownloadFile_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => status.CheckJob(Jobs.Download));
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Jobs.None);
        IObservable<Boolean> treeSelection = this.ItemsTree.RowSelection.WhenAnyValue(selection => selection.SelectedItem).Select(item => item != null && item.Collection == null);

        return Observable.CombineLatest(connectionStatus, jobs, treeSelection, (value1, value2, value3) => value1 && value2 && value3);
    }

    private async Task Execute(Jobs jobDescription)
    {
        using Job job = new Job(jobDescription, this);

        List<ItemViewModel> items = this.ItemsTree.Items.ToList();
        foreach (ItemViewModel item in items)
        {
            await this.Execute(item, jobDescription).ConfigureAwait(true);
        }
    }

    private async Task Execute(ItemViewModel item, Jobs job)
    {
        ConnectionStatusViewModel status = this.ConnectionStatus;

        if (item.Collection != null)
        {
            foreach (ItemViewModel childItem in item.Collection)
            {
                await this.Execute(childItem, job).ConfigureAwait(true);
            }
        }

        if (job.HasFlag(Jobs.Backup) && status.CheckJob(Jobs.Backup)) { await item.Backup().ConfigureAwait(true); }
        if (job.HasFlag(Jobs.Sync) && status.CheckJob(Jobs.Sync)) { await item.Sync().ConfigureAwait(true); }
    }

    private IObservable<Boolean> Execute_CanExecute(Jobs job)
    {
        IObservable<Boolean> backupDirectory = this.WhenAnyValue(vm => vm.HasBackupDirectory).Select(hasBackupDirectory => job != Jobs.Backup || hasBackupDirectory);
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => status.CheckJob((job == Jobs.Sync) ? Jobs.Sync : Jobs.Backup));
        IObservable<Boolean> items = this.WhenAnyValue(vm => vm.HasItems);
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Jobs.None or Jobs.HandwritingRecognition);

        return Observable.CombineLatest(backupDirectory, connectionStatus, items, jobs, (value1, value2, value3, value4) => value1 && value2 && value3 && value4);
    }

    private async Task HandwritingRecognition()
    {
        ItemViewModel? selectedItem = this.ItemsTree.RowSelection.SelectedItem;
        if (selectedItem != null && selectedItem.Collection == null)
        {
            using Job job = new Job(Jobs.HandwritingRecognition, this);

            String text = await selectedItem.HandWritingRecognition().ConfigureAwait(true);

            job.Done();

            await this.ShowDialog.Handle(new HandwritingRecognitionViewModel(text));
        }
    }

    private IObservable<Boolean> HandwritingRecognition_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => status.CheckJob(Jobs.HandwritingRecognition));
        IObservable<Boolean> treeSelection = this.ItemsTree.RowSelection.WhenAnyValue(selection => selection.SelectedItem).Select(item => item != null && item.Collection == null);

        return Observable.CombineLatest(connectionStatus, treeSelection, (value1, value2) => value1 && value2);
    }

    private async Task InstallLamyEraser()
    {
        using Job job = new Job(Jobs.InstallLamyEraser, this);

        await this.ShowDialog.Handle(new LamyEraserViewModel(this.tabletService));
    }

    private IObservable<Boolean> InstallLamyEraser_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => status.CheckJob(Jobs.InstallLamyEraser));
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Jobs.None);

        return Observable.CombineLatest(connectionStatus, jobs, (value1, value2) => value1 && value2);
    }

    private async Task ManageTemplates()
    {
        using Job job = new Job(Jobs.ManageTemplates, this);

        IEnumerable<TemplateData> dataTemplates = await this.dataService.GetTemplates().ConfigureAwait(true);
        IEnumerable<TabletTemplate> tabletTemplates = dataTemplates.Select(template => new TabletTemplate(template.Name, template.Category, template.IconCode, template.BytesPng, template.BytesSvg)).ToArray();
        TemplatesViewModel templates = new TemplatesViewModel(tabletTemplates, this.dataService, this.tabletService);
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
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => status.CheckJob(Jobs.ManageTemplates));
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Jobs.None);

        return Observable.CombineLatest(connectionStatus, jobs, (value1, value2) => value1 && value2);
    }

    private void OpenItem()
    {
        ItemViewModel? selectedItem = this.ItemsTree.RowSelection.SelectedItem;
        if (selectedItem?.CanOpen() == true)
        {
            selectedItem.Open();
        }
    }

    private IObservable<Boolean> OpenItem_CanExecute()
    {
        return Observable.Create<Boolean>(observer =>
        {
            IDisposable? selectedItemObservable = null;

            return this.ItemsTree.RowSelection.WhenAnyValue(s => s.SelectedItem).Subscribe(selectedItem =>
            {
                selectedItemObservable?.Dispose();

                observer.OnNext(selectedItem?.CanOpen() == true);

                selectedItemObservable = selectedItem?.WhenAnyPropertyChanged().Subscribe(item => observer.OnNext(item?.CanOpen() == true));
            });
        });
    }

    private async Task Restart(Job job)
    {
        job.Done();

        String reason = String.Empty;
        if (job.IsJob(Jobs.ManageTemplates) || job.IsJob(Jobs.UploadTemplate))
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
        using Job job = new Job(Jobs.Settings, this);

        await this.ShowDialog.Handle(new SettingsViewModel(this.handWritingRecognitionService, this.tabletService));

        this.HasBackupDirectory = Path.Exists(this.tabletService.Configuration.Backup);
        this.HandWritingRecognitionLanguage = this.HandWritingRecognitionLanguages.Single(language => String.Equals(language.Code, this.handWritingRecognitionService.Configuration.Language, StringComparison.Ordinal));
    }

    private IObservable<Boolean> Settings_CanExecute()
    {
        return this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Jobs.None or Jobs.HandwritingRecognition);
    }

    private async Task SyncTargetDirectory(String setString)
    {
        ItemViewModel? selectedItem = this.ItemsTree.RowSelection.SelectedItem;
        if (selectedItem != null)
        {
            using Job job = new Job(Jobs.SetSyncTargetDirectory, this);

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
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Jobs.None or Jobs.HandwritingRecognition);
        IObservable<Boolean> treeSelection = this.ItemsTree.RowSelection.WhenAnyValue(selection => selection.SelectedItem).Select(item => item != null);

        return Observable.CombineLatest(jobs, treeSelection, (value1, value2) => value1 && value2);
    }

    private async void Update()
    {
        while (true)
        {
            TabletConnectionStatus tabletConnectionStatus = await this.tabletService.GetConnectionStatus().ConfigureAwait(true);
            this.ConnectionStatus = new ConnectionStatusViewModel(tabletConnectionStatus);

            Boolean updated = await this.UpdateItems().ConfigureAwait(true);

            await Task.Delay(TimeSpan.FromSeconds(updated ? 10 : 1)).ConfigureAwait(true);
        }
    }

    private async Task<Boolean> UpdateItems()
    {
        try
        {
            if (this.ConnectionStatus.CheckJob(Jobs.GetItems))
            {
                if (this.Jobs is Jobs.None or Jobs.HandwritingRecognition)
                {
                    using Job? job = this.HasItems ? null : new Job(Jobs.GetItems, this);

                    IEnumerable<TabletItem> tabletItemsAll = await this.tabletService.GetItems().ConfigureAwait(true);
                    IEnumerable<TabletItem> tabletItems = tabletItemsAll.Where(item => !item.Trashed).ToArray();

                    await ItemViewModel.UpdateItems(tabletItems, this.ItemsTree.Items, null, this.dataService, this.handWritingRecognitionService, this.tabletService).ConfigureAwait(true);

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
        using Job job = new Job(Jobs.Upload, this);

        FilePickerOpenOptions options = new FilePickerOpenOptions() { AllowMultiple = true, FileTypeFilter = new[] { FilePickerFileTypes.Pdf, FilePickerFileTypesExtensions.Epub } };
        IEnumerable<String>? files = await this.OpenFilePicker.Handle(options);
        foreach (String file in files)
        {
            ItemViewModel? parentItem = this.ItemsTree.RowSelection.SelectedItem;
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
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => status.CheckJob(Jobs.Upload));
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Jobs.None);

        return Observable.CombineLatest(connectionStatus, jobs, (value1, value2) => value1 && value2);
    }

    private async Task UploadTemplate()
    {
        using Job job = new Job(Jobs.UploadTemplate, this);

        if (await this.ShowDialog.Handle(new TemplateUploadViewModel(this.dataService, this.tabletService)))
        {
            await this.Restart(job).ConfigureAwait(true);
        }
    }

    private IObservable<Boolean> UploadTemplate_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => status.CheckJob(Jobs.UploadTemplate));
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Jobs.None);

        return Observable.CombineLatest(connectionStatus, jobs, (value1, value2) => value1 && value2);
    }

    public ICommand CommandAbout { get; }

    public ICommand CommandBackup { get; }

    public ICommand CommandDownloadFile { get; }

    public ICommand CommandExecute { get; }

    public ICommand CommandHandwritingRecognition { get; }

    public ICommand CommandInstallLamyEraser { get; }

    public ICommand CommandManageTemplates { get; }

    public ICommand CommandOpenItem { get; }

    public ICommand CommandSettings { get; }

    public ICommand CommandSync { get; }

    public ReactiveCommand<String, Unit> CommandSyncTargetDirectory { get; }

    public ICommand CommandUploadFile { get; }

    public ICommand CommandUploadTemplate { get; }

    public ConnectionStatusViewModel ConnectionStatus
    {
        get { return this.connectionStatus; }
        private set { this.RaiseAndSetIfChanged(ref this.connectionStatus, value); }
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

    private Jobs Jobs
    {
        get { return this.jobs; }
        set { this.RaiseAndSetIfChanged(ref this.jobs, value); }
    }

    public String? JobsText
    {
        get { return this.Jobs.GetDisplayText(); }
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
        private Boolean done;
        private readonly Jobs job;
        private readonly MainWindowModel owner;

        public Job(Jobs job, MainWindowModel owner)
        {
            this.done = false;
            this.job = job;
            this.owner = owner;

            this.owner.Jobs |= this.job;
        }

        void IDisposable.Dispose()
        {
            if (!this.done)
            {
                this.done = true;
                this.owner.Jobs ^= this.job;
            }
        }

        public void Done()
        {
            IDisposable disposable = this;
            disposable.Dispose();
        }

        public Boolean IsJob(Jobs job)
        {
            return this.job.HasFlag(job);
        }
    }
}
