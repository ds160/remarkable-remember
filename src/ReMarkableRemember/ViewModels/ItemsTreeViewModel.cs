using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using ReMarkableRemember.Helper;
using ReMarkableRemember.Templates;

namespace ReMarkableRemember.ViewModels;

public sealed class ItemsTreeViewModel : HierarchicalTreeDataGridSource<ItemViewModel>
{
    public ItemsTreeViewModel() : base(new ObservableCollection<ItemViewModel>())
    {
        this.Columns.Add(new HierarchicalExpanderColumn<ItemViewModel>(new TextColumn<ItemViewModel, String>("Name", item => item.Name), item => item.Collection));
        this.Columns.Add(new TextColumn<ItemViewModel, String>("Modified", item => item.Modified.ToDisplayString()));
        this.Columns.Add(new TemplateColumn<ItemViewModel>(null, new ItemHintColumnTemplate(item => null, item => item.CombinedHint)));
        this.Columns.Add(new TextColumn<ItemViewModel, String>("Sync Path", item => item.SyncPath));
        this.Columns.Add(new TemplateColumn<ItemViewModel>("Sync Information", new ItemHintColumnTemplate(item => item.SyncDate, item => item.SyncHint)));
        this.Columns.Add(new TemplateColumn<ItemViewModel>("Backup Information", new ItemHintColumnTemplate(item => item.BackupDate, item => item.BackupHint)));
    }

    public new ObservableCollection<ItemViewModel> Items
    {
        get { return (ObservableCollection<ItemViewModel>)base.Items; }
    }
}
