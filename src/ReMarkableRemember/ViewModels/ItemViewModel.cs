using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ReactiveUI;
using ReMarkableRemember.Common.FileSystem;
using ReMarkableRemember.Common.Notebook;
using ReMarkableRemember.Helper;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.DataService.Models;
using ReMarkableRemember.Services.HandWritingRecognitionService;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.Services.TabletService.Models;
using ReMarkableRemember.Settings;

namespace ReMarkableRemember.ViewModels;

public sealed class ItemViewModel : ViewModelBase
{
    public enum RaiseChangedAdditional
    {
        None,
        Collection,
        Parent
    }

    private readonly IDataService dataService;
    private readonly IHandWritingRecognitionService handWritingRecognitionService;
    private readonly ISettingsService settingsService;
    private readonly ITabletService tabletService;

    internal ItemViewModel(TabletItem tabletItem, ItemViewModel? parent, IDataService dataService, IHandWritingRecognitionService handWritingRecognitionService, ISettingsService settingsService, ITabletService tabletService)
    {
        this.dataService = dataService;
        this.handWritingRecognitionService = handWritingRecognitionService;
        this.settingsService = settingsService;
        this.tabletService = tabletService;

        List<ItemViewModel>? collection = tabletItem.Collection?.Select(childItem => new ItemViewModel(childItem, this, dataService, handWritingRecognitionService, settingsService, tabletService)).ToList();

        this.Collection = (collection != null) ? new OptimizedList<ItemViewModel>(collection) : null;
        this.Parent = parent;
        this.TabletItem = tabletItem;

        this.BackupHint = new ItemHintViewModel.Backup(this, settingsService, tabletService);
        this.SyncHint = new ItemHintViewModel.Sync(this, settingsService);
        // Last to combine backup and sync
        this.CombinedHint = new ItemHintViewModel.Combined(this, settingsService);
    }

    public DateTime? BackupDate { get { return this.DataItem?.BackupDate; } }

    public ItemHintViewModel.Backup BackupHint { get; }

    public OptimizedList<ItemViewModel>? Collection { get; }

    public ItemHintViewModel.Combined CombinedHint { get; }

    internal ItemData? DataItem { get; private set; }

    public String Id { get { return this.TabletItem.Id; } }

    internal DateTime Modified { get { return this.TabletItem.Modified; } }

    public String ModifiedDisplayString { get { return this.TabletItem.Modified.ToDisplayString(this.settingsService); } }

    public String Name { get { return this.TabletItem.Name; } }

    public ItemViewModel? Parent { get; }

    public DateTime? SyncDate { get { return (this.SyncPath != null) ? this.DataItem?.SyncData : null; } }

    public ItemHintViewModel.Sync SyncHint { get; }

    public String? SyncPath { get; private set; }

    private TabletItem TabletItem { get; set; }

    internal async Task Backup()
    {
        if (this.BackupHint.Hint is ItemHintViewModel.Hints.None or >= ItemHintViewModel.Hints.ExistsInTarget) { return; }

        await this.tabletService.Backup(this.Id).ConfigureAwait(true);
        this.DataItem = await this.dataService.SetItemBackup(this.Id, this.Modified).ConfigureAwait(true);

        this.RaiseChanged(RaiseChangedAdditional.Parent);
    }

    internal Boolean CanOpen()
    {
        return Path.Exists(this.SyncPath);
    }

    internal async Task<String> HandWritingRecognition()
    {
        Notebook notebook = await this.tabletService.GetNotebook(this.Id).ConfigureAwait(true);
        IEnumerable<String> pages = await this.handWritingRecognitionService.Recognize(notebook).ConfigureAwait(true);
        return String.Join(Environment.NewLine, pages);
    }

    internal void Open()
    {
        if (Path.Exists(this.SyncPath))
        {
            Process.Start(new ProcessStartInfo(this.SyncPath) { UseShellExecute = true });
        }
    }

    internal void RaiseChanged(RaiseChangedAdditional additional)
    {
        this.BackupHint.RaiseChanged();
        this.SyncHint.RaiseChanged();
        // Last to combine backup and sync
        this.CombinedHint.RaiseChanged();

        foreach (PropertyInfo property in this.GetType().GetProperties())
        {
            this.RaisePropertyChanged(property.Name);
        }

        if (additional == RaiseChangedAdditional.Collection) { this.Collection?.ToList()?.ForEach(item => item.RaiseChanged(additional)); }
        if (additional == RaiseChangedAdditional.Parent) { this.Parent?.RaiseChanged(additional); }
    }

    internal async Task SetSyncTargetDirectory(String? targetDirectory)
    {
        this.DataItem = await this.dataService.SetItemSyncTargetDirectory(this.Id, targetDirectory).ConfigureAwait(true);

        await this.Update().ConfigureAwait(true);
        this.RaiseChanged(RaiseChangedAdditional.Parent);
    }

    internal async Task Sync()
    {
        if (this.SyncHint.Hint is ItemHintViewModel.Hints.None or >= ItemHintViewModel.Hints.ExistsInTarget) { return; }
        if (this.SyncPath == null) { return; }

        if (this.DataItem != null && this.DataItem.SyncPath != null && this.SyncHint.Hint.HasFlag(ItemHintViewModel.Hints.SyncPathChanged))
        {
            FileSystem.Delete(this.DataItem.SyncPath);
        }

        await this.tabletService.Download(this.Id, this.SyncPath).ConfigureAwait(true);
        this.DataItem = await this.dataService.SetItemSync(this.Id, this.Modified, this.SyncPath).ConfigureAwait(true);

        this.RaiseChanged(RaiseChangedAdditional.Parent);
    }

    private async Task Update()
    {
        this.DataItem = await this.dataService.GetItem(this.Id).ConfigureAwait(true);

        String? targetDirectory = null;
        if (this.DataItem != null && this.DataItem.SyncTargetDirectory != null)
        {
            targetDirectory = this.DataItem.SyncTargetDirectory;
        }
        else if (this.Parent != null && this.Parent.SyncPath != null)
        {
            targetDirectory = (this.Collection != null) ? Path.Combine(this.Parent.SyncPath, this.Name) : this.Parent.SyncPath;
        }
        this.SyncPath = (targetDirectory != null && this.Collection == null) ? Path.Combine(targetDirectory, this.Name) : targetDirectory;

        this.RaiseChanged(RaiseChangedAdditional.None);

        if (this.Collection != null)
        {
            await Task.WhenAll(this.Collection.Select(childItem => childItem.Update())).ConfigureAwait(true);
        }
    }

    internal static async Task UpdateItems(IEnumerable<TabletItem> tabletItems, OptimizedList<ItemViewModel> items, ItemViewModel? parentItem, IDataService dataService, IHandWritingRecognitionService handWritingRecognitionService, ISettingsService settingsService, ITabletService tabletService)
    {
        List<ItemViewModel> itemsToAdd = new List<ItemViewModel>();
        foreach (TabletItem tabletItem in tabletItems)
        {
            ItemViewModel? item = items.SingleOrDefault(item => item.TabletItem.Id == tabletItem.Id);
            if (item == null)
            {
                item = new ItemViewModel(tabletItem, parentItem, dataService, handWritingRecognitionService, settingsService, tabletService);
                await item.Update().ConfigureAwait(true);
                itemsToAdd.Add(item);
            }
            else
            {
                item.TabletItem = tabletItem;

                if (tabletItem.Collection != null && item.Collection != null)
                {
                    await UpdateItems(tabletItem.Collection, item.Collection, item, dataService, handWritingRecognitionService, settingsService, tabletService).ConfigureAwait(true);
                }

                if (parentItem == null)
                {
                    item.RaiseChanged(RaiseChangedAdditional.Collection);
                }
            }
        }
        items.AddRange(itemsToAdd);

        List<ItemViewModel> itemsToRemove = items.Where(item => !tabletItems.Any(sourceItem => item.TabletItem.Id == sourceItem.Id)).ToList();
        items.RemoveRange(itemsToRemove);
    }
}
