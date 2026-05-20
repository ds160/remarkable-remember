using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using DynamicData.Binding;
using ReactiveUI;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.DependencyInjection;
using ReMarkableRemember.Helper;
using ReMarkableRemember.Services.DataService.Models;
using ReMarkableRemember.Services.HandWritingRecognitionService.Configuration;
using ReMarkableRemember.Services.TabletService.Models;
using ReMarkableRemember.ViewModels.Enumerations;
using ReMarkableRemember.ViewModels.Interfaces;

namespace ReMarkableRemember.ViewModels;

public sealed partial class MainWindowModel : ViewModelBase, IAppModel
{
    private readonly IServices services;

    private String? itemsNotReadable;

    private MainWindowModel(IServices services)
    {
        this.services = services;

        this.ItemsTree = new ItemsTreeViewModel();
        this.HandWritingRecognitionLanguages = HandWritingRecognitionLanguageViewModel.GetLanguages(this.services);
        this.OpenFilePicker = new Interaction<FilePickerOpenOptions, IEnumerable<String>?>();
        this.OpenFolderPicker = new Interaction<String, String?>();
        this.OpenSaveFilePicker = new Interaction<FilePickerSaveOptions, String?>();
        this.ShowDialog = new Interaction<DialogWindowModel, Boolean>();

        this.ApplicationTheme = this.services.Settings.Configuration.ApplicationTheme;
        this.ConnectionStatus = new ConnectionStatusViewModel();
        this.HandWritingRecognitionLanguage = this.HandWritingRecognitionLanguages.Single(language => String.Equals(language.Code, this.services.HandWritingRecognition.Configuration.Language, StringComparison.Ordinal));
        this.HasBackupDirectory = Path.Exists(this.services.Tablet.Configuration.Backup);
        this.Jobs = Jobs.None;

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
    }


    public static MainWindowModel Create(IServices services)
    {
        MainWindowModel mainWindowModel = new MainWindowModel(services);
        RxSchedulers.MainThreadScheduler.Schedule(mainWindowModel.Update);
        return mainWindowModel;
    }


    private async Task About()
    {
        await this.ShowDialog.Handle(new AboutViewModel());
    }

    private async Task DownloadFile()
    {
        ItemViewModel? selectedItem = this.ItemsTree.SelectedItem;
        if (selectedItem != null && selectedItem.Collection == null)
        {
            using Job job = new Job(Jobs.Download, this);

            FilePickerSaveOptions options = new FilePickerSaveOptions()
            {
                DefaultExtension = "pdf",
                FileTypeChoices = new[] { FilePickerTypes.Pdf },
                SuggestedFileName = selectedItem.Name
            };
            String? targetPath = await this.OpenSaveFilePicker.Handle(options);
            if (targetPath != null)
            {
                await this.services.Tablet.Download(selectedItem.Id, targetPath);
            }
        }
    }

    private IObservable<Boolean> DownloadFile_CanExecute()
    {
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => status.CheckJob(Jobs.Download));
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Jobs.None);
        IObservable<Boolean> treeSelection = this.ItemsTree.WhenAnyValue(itemTree => itemTree.SelectedItem).Select(item => item != null && item.Collection == null);

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
        IObservable<Boolean> items = this.ItemsTree.Items.WhenAnyValue(vm => vm.Count).Select(count => count > 0);
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Jobs.None or Jobs.HandwritingRecognition);

        return Observable.CombineLatest(backupDirectory, connectionStatus, items, jobs, (value1, value2, value3, value4) => value1 && value2 && value3 && value4);
    }

    private async Task HandwritingRecognition()
    {
        ItemViewModel? selectedItem = this.ItemsTree.SelectedItem;
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
        IObservable<Boolean> treeSelection = this.ItemsTree.WhenAnyValue(itemTree => itemTree.SelectedItem).Select(item => item != null && item.Collection == null);

        return Observable.CombineLatest(connectionStatus, treeSelection, (value1, value2) => value1 && value2);
    }

    private async Task InstallLamyEraser()
    {
        using Job job = new Job(Jobs.InstallLamyEraser, this);

        await this.ShowDialog.Handle(new LamyEraserViewModel(this.services));
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

        IEnumerable<TemplateData> dataTemplates = await this.services.Data.GetTemplates().ConfigureAwait(true);
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
        IObservable<Boolean> connectionStatus = this.WhenAnyValue(vm => vm.ConnectionStatus).Select(status => status.CheckJob(Jobs.ManageTemplates));
        IObservable<Boolean> jobs = this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Jobs.None);

        return Observable.CombineLatest(connectionStatus, jobs, (value1, value2) => value1 && value2);
    }

    private void OpenItem()
    {
        ItemViewModel? selectedItem = this.ItemsTree.SelectedItem;
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

            return this.ItemsTree.WhenAnyValue(itemTree => itemTree.SelectedItem).Subscribe(selectedItem =>
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
        if (job.Is(Jobs.ManageTemplates) || job.Is(Jobs.UploadTemplate))
        {
            reason = $"{Language.Current.TabletRestartReasonTemplate} ";
        }

        MessageViewModel message = MessageViewModel.Question(Language.Current.TabletRestartTitle, String.Join(Environment.NewLine,
            $"{reason}{Language.Current.TabletRestartTakeEffect}",
            Language.Current.TabletRestartSaveWork,
            String.Empty,
            Language.Current.TabletRestartQuestion),
            this.services);

        if (await this.ShowDialog.Handle(message))
        {
            await this.services.Tablet.Restart().ConfigureAwait(true);
        }
    }

    private async void SaveHandWritingRecognitionLanguage(HandWritingRecognitionLanguageViewModel language)
    {
        IHandWritingRecognitionConfiguration configuration = this.services.HandWritingRecognition.Configuration;
        configuration.Language = language.Code;
        await configuration.Save().ConfigureAwait(true);
    }

    private async Task Settings()
    {
        using Job job = new Job(Jobs.Settings, this);

        if (await this.ShowDialog.Handle(new SettingsViewModel(this.services)))
        {
            // Update localized strings
            this.ConnectionStatus.UpdateLocalizedText();
            this.HandWritingRecognitionLanguages = HandWritingRecognitionLanguageViewModel.GetLanguages(this.services);
            foreach (ItemViewModel item in this.ItemsTree.Items) { item.UpdateLocalizedText(); }
            this.RaisePropertyChanged(nameof(this.JobsText));
            this.RaisePropertyChanged(nameof(this.LocalStrings));

            // Update properties
            this.ApplicationTheme = this.services.Settings.Configuration.ApplicationTheme;
            this.HasBackupDirectory = Path.Exists(this.services.Tablet.Configuration.Backup);
            this.HandWritingRecognitionLanguage = this.HandWritingRecognitionLanguages.Single(language => String.Equals(language.Code, this.services.HandWritingRecognition.Configuration.Language, StringComparison.Ordinal));
        }
    }

    private IObservable<Boolean> Settings_CanExecute()
    {
        return this.WhenAnyValue(vm => vm.Jobs).Select(jobs => jobs is Jobs.None or Jobs.HandwritingRecognition);
    }

    private async Task SyncTargetDirectory(String setString)
    {
        ItemViewModel? selectedItem = this.ItemsTree.SelectedItem;
        if (selectedItem != null)
        {
            using Job job = new Job(Jobs.SetSyncTargetDirectory, this);

            if (Boolean.TryParse(setString, out Boolean set) && set)
            {
                String? targetDirectory = await this.OpenFolderPicker.Handle(Language.Current.ItemSyncTargetFolder);
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
        IObservable<Boolean> treeSelection = this.ItemsTree.WhenAnyValue(itemTree => itemTree.SelectedItem).Select(item => item != null);

        return Observable.CombineLatest(jobs, treeSelection, (value1, value2) => value1 && value2);
    }

    private async void Update()
    {
        while (true)
        {
            Boolean updated = await this.UpdateItems().ConfigureAwait(true);

            await Task.Delay(TimeSpan.FromSeconds(updated ? 10 : 1)).ConfigureAwait(true);
        }
    }

    private async Task<Boolean> UpdateItems()
    {
        try
        {
            TabletConnectionStatus tabletConnectionStatus = await this.services.Tablet.GetConnectionStatus().ConfigureAwait(true);
            this.ConnectionStatus = new ConnectionStatusViewModel(tabletConnectionStatus);
            if (this.ConnectionStatus.CheckJob(Jobs.GetItems))
            {
                Boolean update = this.Jobs is Jobs.None or Jobs.HandwritingRecognition;
                if (update)
                {
                    using Job? job = this.ItemsTree.Items.Count > 0 ? null : new Job(Jobs.GetItems, this);

                    TabletItems tabletItems = await this.services.Tablet.GetItems().ConfigureAwait(true);
                    IEnumerable<TabletItem> tabletItemsNotTrashed = tabletItems.Items.Where(item => !item.Trashed).ToArray();
                    await ItemViewModel.UpdateItems(tabletItemsNotTrashed, this.ItemsTree.Items, null, this.services).ConfigureAwait(true);
                    await this.UpdateItemsNotReadable(tabletItems.NotReadable).ConfigureAwait(true);
                }
                return update;
            }
        }
        catch (Exception exception)
        {
            await this.ShowDialog.Handle(MessageViewModel.Error(exception, this.services));
        }

        this.ItemsTree.Items.Clear();
        return false;
    }

    private async Task UpdateItemsNotReadable(IEnumerable<String> notReadable)
    {
        String itemsNotReadable = String.Join(Environment.NewLine, notReadable);
        if (!String.IsNullOrEmpty(itemsNotReadable) && !String.Equals(itemsNotReadable, this.itemsNotReadable, StringComparison.Ordinal))
        {
            await this.ShowDialog.Handle(MessageViewModel.Error($"{Language.Current.TabletItemsNotReadable}{Environment.NewLine}{itemsNotReadable}", this.services));
        }
        this.itemsNotReadable = itemsNotReadable;
    }

    private async Task UploadFile()
    {
        using Job job = new Job(Jobs.Upload, this);

        FilePickerOpenOptions options = new FilePickerOpenOptions() { AllowMultiple = true, FileTypeFilter = new[] { FilePickerTypes.Pdf, FilePickerTypes.Epub } };
        IEnumerable<String>? files = await this.OpenFilePicker.Handle(options);
        String? parentId = this.UploadFileParentId();
        foreach (String file in files)
        {
            await this.services.Tablet.UploadFile(file, parentId).ConfigureAwait(true);
        }
    }

    private String? UploadFileParentId()
    {
        ItemViewModel? parentItem = this.ItemsTree.SelectedItem;

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

        if (await this.ShowDialog.Handle(new TemplateUploadViewModel(this.services)))
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

    public ICommand CommandSyncTargetDirectory { get; }

    public ICommand CommandUploadFile { get; }

    public ICommand CommandUploadTemplate { get; }

    public String ApplicationTheme { get; set { this.RaiseAndSetIfChanged(ref field, value); } }

    public ConnectionStatusViewModel ConnectionStatus { get; private set { this.RaiseAndSetIfChanged(ref field, value); } }

    public ItemsTreeViewModel ItemsTree { get; }

    internal Boolean HasBackupDirectory { get; private set { this.RaiseAndSetIfChanged(ref field, value); } }

    internal Jobs Jobs { get; private set { this.RaiseAndSetIfChanged(ref field, value); } }

    public String? JobsText { get { return this.Jobs.GetDisplayText(); } }

    public HandWritingRecognitionLanguageViewModel HandWritingRecognitionLanguage { get; set { this.RaiseAndSetIfChanged(ref field, value); } }

    public IEnumerable<HandWritingRecognitionLanguageViewModel> HandWritingRecognitionLanguages { get; private set { this.RaiseAndSetIfChanged(ref field, value); } }

    public Interaction<FilePickerOpenOptions, IEnumerable<String>?> OpenFilePicker { get; }

    public Interaction<String, String?> OpenFolderPicker { get; }

    public Interaction<FilePickerSaveOptions, String?> OpenSaveFilePicker { get; }

    public Interaction<DialogWindowModel, Boolean> ShowDialog { get; }

    public static String Title { get { return $"reMarkable Remember - {Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)}"; } }
}
