using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls.DataGridHierarchical;
using Avalonia.Controls.Selection;
using ReactiveUI;

namespace ReMarkableRemember.ViewModels;

public sealed class ItemsTreeViewModel : ViewModelBase
{
    public ItemsTreeViewModel()
    {
        HierarchicalOptions<ItemViewModel> hierarchicalOptions = new HierarchicalOptions<ItemViewModel>()
        {
            ItemsSelector = item => item.Collection,
            IsLeafSelector = item => item.Collection is null,
            VirtualizeChildren = true
        };

        this.HierarchicalModel = new HierarchicalModel<ItemViewModel>(hierarchicalOptions);
        this.Items = new ObservableCollection<ItemViewModel>();
        this.Selection = new SelectionModel<HierarchicalNode>();

        this.HierarchicalModel.ApplySiblingComparer(Comparer<ItemViewModel>.Create(ItemViewModel.Compare));
        this.HierarchicalModel.SetRoots(this.Items);
        this.Selection.SelectionChanged += (_, _) => this.RaisePropertyChanged(nameof(this.SelectedItem));
    }

    public HierarchicalModel<ItemViewModel> HierarchicalModel { get; }

    internal ObservableCollection<ItemViewModel> Items { get; }

    internal ItemViewModel? SelectedItem { get { return this.Selection.SelectedItem?.Item as ItemViewModel; } }

    public SelectionModel<HierarchicalNode> Selection { get; }

    internal void UpdateLocalizedText()
    {
        foreach (ItemViewModel item in this.Items)
        {
            item.RaiseChanged(ItemViewModel.RaiseChangedAdditional.Collection);
        }
    }
}
