using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReMarkableRemember.Common.FileSystem;
using ReMarkableRemember.Common.Notebook;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.DataService.Models;
using ReMarkableRemember.Services.HandWritingRecognition;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class ItemViewModel : ViewModelBase
{
    [Flags]
    public enum Hint
    {
        None = 0x00,
        NotFoundInTarget = 0x01,
        SyncPathChanged = 0x02,
        Modified = 0x04,
        New = 0x08,
        ExistsInTarget = 0x10
    }

    public enum Image
    {
        None,
        Green,
        Yellow,
        Red
    }

    public enum RaiseChangedAdditional
    {
        None,
        Collection,
        Parent
    }

    private readonly IDataService dataService;
    private readonly IHandWritingRecognitionService handWritingRecognitionService;
    private readonly ITabletService tabletService;

    internal ItemViewModel(TabletItem tabletItem, ItemViewModel? parent, ServiceProvider services)
    {
        this.dataService = services.GetRequiredService<IDataService>();
        this.handWritingRecognitionService = services.GetRequiredService<IHandWritingRecognitionService>();
        this.tabletService = services.GetRequiredService<ITabletService>();

        List<ItemViewModel>? collection = tabletItem.Collection?.Select(childItem => new ItemViewModel(childItem, this, services)).ToList();

        this.Collection = (collection != null) ? new ObservableCollection<ItemViewModel>(collection) : null;
        this.Parent = parent;
        this.TabletItem = tabletItem;
    }

    public DateTime? BackupDate { get { return this.DataItem?.BackupDate; } }

    public Hint BackupHint
    {
        get
        {
            if (!Path.Exists(this.tabletService.Configuration.Backup)) { return Hint.None; }
            if (this.DataItem == null) { return Hint.None; }

            if (this.DataItem.BackupDate == null) { return Hint.New; }
            if (this.DataItem.BackupDate < this.Modified) { return Hint.Modified; }

            return Hint.None;
        }
    }

    public ObservableCollection<ItemViewModel>? Collection { get; }

    public Hint CombinedHint { get { return GetCombinedHint(this); } }

    private ItemData? DataItem { get; set; }

    public String Id { get { return this.TabletItem.Id; } }

    public DateTime Modified { get { return this.TabletItem.Modified; } }

    public String Name { get { return this.TabletItem.Name; } }

    public ItemViewModel? Parent { get; }

    public DateTime? SyncDate { get { return (this.SyncPath != null) ? this.DataItem?.SyncData : null; } }

    public Hint SyncHint
    {
        get
        {
            if (this.Collection != null) { return Hint.None; }
            if (this.SyncPath == null) { return Hint.None; }
            if (this.DataItem == null) { return Hint.None; }

            if (this.DataItem.SyncPath == null && Path.Exists(this.SyncPath)) { return Hint.ExistsInTarget; }
            if (this.DataItem.SyncPath == null) { return Hint.New; }
            if (this.DataItem.SyncPath != this.SyncPath) { return Hint.SyncPathChanged; }
            if (this.DataItem.SyncData < this.Modified) { return Hint.Modified; }
            if (!Path.Exists(this.SyncPath)) { return Hint.NotFoundInTarget; }

            return Hint.None;
        }
    }

    public String? SyncPath { get; private set; }

    private TabletItem TabletItem { get; set; }

    internal async Task Backup()
    {
        if (this.BackupHint is Hint.None or >= Hint.ExistsInTarget) { return; }

        await this.tabletService.Backup(this.Id).ConfigureAwait(true);
        this.DataItem = await this.dataService.SetItemBackup(this.Id, this.Modified).ConfigureAwait(true);

        this.RaiseChanged(RaiseChangedAdditional.Parent);
    }

    internal async Task<String> HandWritingRecognition()
    {
        Notebook notebook = await this.tabletService.GetNotebook(this.Id).ConfigureAwait(true);
        IEnumerable<String> pages = await Task.WhenAll(notebook.Pages.Select(page => this.handWritingRecognitionService.Recognize(page))).ConfigureAwait(true);
        return String.Join(Environment.NewLine, pages);
    }

    private void RaiseChanged(RaiseChangedAdditional additional)
    {
        this.RaisePropertyChanged();

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
        if (this.SyncHint is Hint.None or >= Hint.ExistsInTarget) { return; }
        if (this.SyncPath == null) { return; }

        if (this.DataItem != null && this.DataItem.SyncPath != null && this.SyncHint.HasFlag(Hint.SyncPathChanged))
        {
            FileSystem.Delete(this.DataItem.SyncPath);
        }

        await this.tabletService.Download(this.Id, this.SyncPath).ConfigureAwait(true);
        this.DataItem = await this.dataService.SetItemSync(this.Id, this.Modified, this.SyncPath).ConfigureAwait(true);

        this.RaiseChanged(RaiseChangedAdditional.Parent);
    }

    private async Task Update()
    {
        this.DataItem = await this.dataService.GetItem(this.Id);

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

    internal static Int32 Compare(ItemViewModel itemA, ItemViewModel itemB)
    {
        Int32 collectionA = (itemA.Collection == null) ? 1 : 0;
        Int32 collectionB = (itemB.Collection == null) ? 1 : 0;
        Int32 collectionCompareResult = collectionA - collectionB;

        return (collectionCompareResult != 0) ? collectionCompareResult : String.CompareOrdinal(itemA.Name, itemB.Name);
    }

    private static Hint GetCombinedHint(ItemViewModel item)
    {
        Hint hint = item.BackupHint | item.SyncHint;

        if (item.Collection != null)
        {
            foreach (ItemViewModel childItem in item.Collection)
            {
                hint |= GetCombinedHint(childItem);
            }
        }

        return hint;
    }

    public static Image GetImage(DateTime? dateTime, Hint hint)
    {
        if (hint.HasFlag(Hint.ExistsInTarget)) { return Image.Red; }
        if (hint.HasFlag(Hint.New)) { return Image.Yellow; }
        if (hint.HasFlag(Hint.Modified)) { return Image.Yellow; }
        if (hint.HasFlag(Hint.SyncPathChanged)) { return Image.Yellow; }
        if (hint.HasFlag(Hint.NotFoundInTarget)) { return Image.Yellow; }

        if (hint == Hint.None) { return (dateTime != null) ? Image.Green : Image.None; }

        throw new NotImplementedException();
    }

    public static String? GetToolTip(DateTime? dateTime, Hint hint)
    {
        if (hint.HasFlag(Hint.ExistsInTarget)) { return "Exists already in target directory"; }
        if (hint.HasFlag(Hint.New)) { return "New"; }
        if (hint.HasFlag(Hint.Modified)) { return "Modified"; }
        if (hint.HasFlag(Hint.SyncPathChanged)) { return "Sync path changed"; }
        if (hint.HasFlag(Hint.NotFoundInTarget)) { return "Not found in target directory"; }

        if (hint == Hint.None) { return (dateTime != null) ? "Up-to-date" : null; }

        throw new NotImplementedException();
    }

    internal static async Task UpdateItems(IEnumerable<TabletItem> tabletItems, ObservableCollection<ItemViewModel> items, ItemViewModel? parentItem, ServiceProvider services)
    {
        foreach (TabletItem tabletItem in tabletItems)
        {
            ItemViewModel? item = items.SingleOrDefault(item => item.TabletItem.Id == tabletItem.Id);
            if (item == null)
            {
                item = new ItemViewModel(tabletItem, parentItem, services);
                await item.Update().ConfigureAwait(true);
                items.Add(item);
            }
            else
            {
                item.TabletItem = tabletItem;

                if (tabletItem.Collection != null && item.Collection != null)
                {
                    await UpdateItems(tabletItem.Collection, item.Collection, item, services).ConfigureAwait(true);
                }

                if (parentItem == null)
                {
                    item.RaiseChanged(RaiseChangedAdditional.Collection);
                }
            }
        }

        List<ItemViewModel> itemsToRemove = items.Where(item => !tabletItems.Any(sourceItem => item.TabletItem.Id == sourceItem.Id)).ToList();
        itemsToRemove.ForEach(itemToRemove => items.Remove(itemToRemove));
    }
}
