using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Helper;
using ReMarkableRemember.Templates;

namespace ReMarkableRemember.ViewModels;

public sealed class ItemsTreeViewModel : HierarchicalTreeDataGridSource<ItemViewModel>
{
    public ItemsTreeViewModel() : base(new ObservableCollection<ItemViewModel>())
    {
        this.Columns.Add(new HierarchicalExpanderColumn<ItemViewModel>(new TextColumn<ItemViewModel, String>(null, item => item.Name), item => item.Collection) { Tag = () => Language.Current.ItemsTreeViewName });
        this.Columns.Add(new TextColumn<ItemViewModel, String>(null, item => item.Modified.ToDisplayString()) { Tag = () => Language.Current.ItemsTreeViewModified });
        this.Columns.Add(new TemplateColumn<ItemViewModel>(null, new ItemHintColumnTemplate(item => null, item => item.CombinedHint)));
        this.Columns.Add(new TextColumn<ItemViewModel, String>(null, item => item.SyncPath) { Tag = () => Language.Current.ItemsTreeViewSyncPath });
        this.Columns.Add(new TemplateColumn<ItemViewModel>(null, new ItemHintColumnTemplate(item => item.SyncDate, item => item.SyncHint)) { Tag = () => Language.Current.ItemsTreeViewSyncInformation });
        this.Columns.Add(new TemplateColumn<ItemViewModel>(null, new ItemHintColumnTemplate(item => item.BackupDate, item => item.BackupHint)) { Tag = () => Language.Current.ItemsTreeViewBackupInformation });

        this.SetLocalizedHeaders();
    }

    public new ObservableCollection<ItemViewModel> Items
    {
        get { return (ObservableCollection<ItemViewModel>)base.Items; }
    }

    public new ITreeDataGridRowSelectionModel<ItemViewModel> RowSelection
    {
        get { return base.RowSelection!; }
    }

    public void SetLocalizedHeaders()
    {
        foreach (IColumn<ItemViewModel> column in this.Columns)
        {
            if (column is HierarchicalExpanderColumn<ItemViewModel> hierarchicalExpanderColumn)
            {
                SetLocalizedHeader(hierarchicalExpanderColumn.Inner);
            }
            else
            {
                SetLocalizedHeader(column);
            }
        }
    }

    private static void SetLocalizedHeader(IColumn<ItemViewModel> column)
    {
        if (column is ColumnBase<ItemViewModel> columnBase && columnBase.Tag is Func<String> action)
        {
            columnBase.Header = action.Invoke();
        }
    }
}
