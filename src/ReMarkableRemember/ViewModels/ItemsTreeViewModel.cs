using System;
using System.Collections.Generic;
using Avalonia.Controls.DataGridHierarchical;
using Avalonia.Controls.Selection;
using ReactiveUI;
using ReMarkableRemember.Helper;

namespace ReMarkableRemember.ViewModels;

public sealed class ItemsTreeViewModel : ViewModelBase
{
    public ItemsTreeViewModel()
    {
        HierarchicalOptions<ItemViewModel> hierarchicalOptions = new HierarchicalOptions<ItemViewModel>()
        {
            ItemsSelector = item => item.Collection,
            IsLeafSelector = item => item.Collection is null
        };

        this.HierarchicalModel = new HierarchicalModel<ItemViewModel>(hierarchicalOptions);
        this.Items = new OptimizedList<ItemViewModel>();
        this.Selection = new SelectionModel<HierarchicalNode>();

        this.HierarchicalModel.ApplySiblingComparer(Comparer<ItemViewModel>.Create(Compare));
        this.HierarchicalModel.SetRoots(this.Items);
        this.Selection.SelectionChanged += (_, _) => this.RaisePropertyChanged(nameof(this.SelectedItem));
    }

    public HierarchicalModel<ItemViewModel> HierarchicalModel { get; }

    internal OptimizedList<ItemViewModel> Items { get; }

    internal ItemViewModel? SelectedItem { get { return this.Selection.SelectedItem?.Item as ItemViewModel; } }

    public SelectionModel<HierarchicalNode> Selection { get; }

    private static Int32 Compare(ItemViewModel x, ItemViewModel y)
    {
        Int32 collectionX = (x.Collection == null) ? 1 : 0;
        Int32 collectionY = (y.Collection == null) ? 1 : 0;
        Int32 collectionCompareResult = collectionX - collectionY;

        return (collectionCompareResult != 0) ? collectionCompareResult : String.CompareOrdinal(x.Name, y.Name);
    }
}
