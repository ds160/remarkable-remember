using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.ViewModels;

internal sealed class ItemViewModel : ViewModelBase
{
    [Flags]
    public enum Hint
    {
        None = Item.Hint.None,
        NotFoundInTarget = Item.Hint.NotFoundInTarget,
        SyncPathChanged = Item.Hint.SyncPathChanged,
        Modified = Item.Hint.Modified,
        New = Item.Hint.New,
        ExistsInTarget = Item.Hint.ExistsInTarget
    }

    public ItemViewModel(Item source, ItemViewModel? parent)
    {
        List<ItemViewModel>? collection = source.Collection?.Select(childItem => new ItemViewModel(childItem, this)).ToList();
        collection?.Sort(new Comparison<ItemViewModel>(Compare));

        this.Collection = collection;
        this.Parent = parent;
        this.Source = source;
    }

    public DateTime? Backup { get { return this.Source.Backup; } }

    public Hint BackupHint { get { return (Hint)this.Source.BackupHint; } }

    public IEnumerable<ItemViewModel>? Collection { get; }

    public Hint CombinedHint { get { return GetCombinedHint(this); } }

    public DateTime Modified { get { return this.Source.Modified; } }

    public String Name { get { return this.Source.Name; } }

    public ItemViewModel? Parent { get; }

    public DateTime? Sync { get { return this.Source?.Sync?.Modified; } }

    public String? SyncPath { get { return this.Source.SyncPath; } }

    public Hint SyncHint { get { return (Hint)this.Source.SyncHint; } }

    internal Item Source { get; }

    public void RaiseChanged()
    {
        this.RaisePropertyChanged();
        this.Parent?.RaiseChanged();
    }

    public static Int32 Compare(ItemViewModel itemA, ItemViewModel itemB)
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
}
