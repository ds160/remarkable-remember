using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class ItemViewModel : ViewModelBase
{
    [Flags]
    public enum Hint
    {
        None = ItemHint.None,
        NotFoundInTarget = ItemHint.NotFoundInTarget,
        SyncPathChanged = ItemHint.SyncPathChanged,
        Modified = ItemHint.Modified,
        New = ItemHint.New,
        ExistsInTarget = ItemHint.ExistsInTarget
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

    internal ItemViewModel(Item source, ItemViewModel? parent)
    {
        List<ItemViewModel>? collection = source.Collection?.Select(childItem => new ItemViewModel(childItem, this)).ToList();

        this.Collection = (collection != null) ? new ObservableCollection<ItemViewModel>(collection) : null;
        this.Parent = parent;
        this.Source = source;
    }

    public DateTime? Backup { get { return this.Source.BackupDate; } }

    public Hint BackupHint { get { return (Hint)this.Source.BackupHint; } }

    public ObservableCollection<ItemViewModel>? Collection { get; }

    public Hint CombinedHint { get { return GetCombinedHint(this); } }

    public DateTime Modified { get { return this.Source.Modified; } }

    public String Name { get { return this.Source.Name; } }

    public ItemViewModel? Parent { get; }

    public DateTime? Sync { get { return (this.SyncPath != null) ? this.Source?.SyncDate : null; } }

    public Hint SyncHint { get { return (Hint)this.Source.SyncHint; } }

    public String? SyncPath { get { return this.Source.SyncPath; } }

    internal Item Source { get; set; }

    internal void RaiseChanged(RaiseChangedAdditional additional)
    {
        this.RaisePropertyChanged();

        if (additional == RaiseChangedAdditional.Collection) { this.Collection?.ToList()?.ForEach(item => item.RaiseChanged(additional)); }
        if (additional == RaiseChangedAdditional.Parent) { this.Parent?.RaiseChanged(additional); }
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

    internal static void UpdateItems(IEnumerable<Item> sourceItems, ObservableCollection<ItemViewModel> items, ItemViewModel? parentItem)
    {
        foreach (Item sourceItem in sourceItems)
        {
            ItemViewModel? item = items.SingleOrDefault(item => item.Source.Id == sourceItem.Id);
            if (item == null)
            {
                items.Add(new ItemViewModel(sourceItem, parentItem));
            }
            else
            {
                item.Source = sourceItem;

                if (sourceItem.Collection != null && item.Collection != null)
                {
                    UpdateItems(sourceItem.Collection, item.Collection, item);
                }

                if (parentItem == null)
                {
                    item.RaiseChanged(RaiseChangedAdditional.Collection);
                }
            }
        }

        List<ItemViewModel> itemsToRemove = items.Where(item => !sourceItems.Any(sourceItem => item.Source.Id == sourceItem.Id)).ToList();
        itemsToRemove.ForEach(itemToRemove => items.Remove(itemToRemove));
    }
}
