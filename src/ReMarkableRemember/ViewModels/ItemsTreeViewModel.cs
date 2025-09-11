using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using DynamicData.Binding;
using ReactiveUI;
using ReMarkableRemember.Helper;
using ReMarkableRemember.Templates;

namespace ReMarkableRemember.ViewModels;

public sealed class ItemsTreeViewModel : HierarchicalTreeDataGridSource<ItemViewModel>
{
    private IDisposable? selectedItemObservable;

    public ItemsTreeViewModel() : base(new ObservableCollection<ItemViewModel>())
    {
        this.Columns.Add(new HierarchicalExpanderColumn<ItemViewModel>(new TextColumn<ItemViewModel, String>("Name", item => item.Name), item => item.Collection));
        this.Columns.Add(new TextColumn<ItemViewModel, String>("Modified", item => item.Modified.ToDisplayString()));
        this.Columns.Add(new TemplateColumn<ItemViewModel>(null, new ItemHintColumnTemplate(item => null, item => item.CombinedHint)));
        this.Columns.Add(new TextColumn<ItemViewModel, String>("Sync Path", item => item.SyncPath));
        this.Columns.Add(new TemplateColumn<ItemViewModel>("Sync Information", new ItemHintColumnTemplate(item => item.SyncDate, item => item.SyncHint)));
        this.Columns.Add(new TemplateColumn<ItemViewModel>("Backup Information", new ItemHintColumnTemplate(item => item.BackupDate, item => item.BackupHint)));

        this.RowSelection.WhenAnyValue(s => s.SelectedItem).Subscribe(this.OnSelectedItemChanged);
    }

    public new ObservableCollection<ItemViewModel> Items
    {
        get { return (ObservableCollection<ItemViewModel>)base.Items; }
    }

    public new ITreeDataGridRowSelectionModel<ItemViewModel> RowSelection
    {
        get { return base.RowSelection!; }
    }

    private void OnSelectedItemChanged(ItemViewModel? selectedItem)
    {
        this.selectedItemObservable?.Dispose();
        this.selectedItemObservable = null;

        if (selectedItem == null) { return; }

        this.selectedItemObservable = selectedItem.WhenAnyPropertyChanged().Subscribe(_ =>
        {
            IndexPath indexPath = this.RowSelection.SelectedIndex;

            this.RowSelection.Clear();
            this.RowSelection.Select(indexPath);
        });
    }
}
