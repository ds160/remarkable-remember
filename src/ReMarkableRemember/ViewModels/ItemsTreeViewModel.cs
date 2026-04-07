using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Helper;
using ReMarkableRemember.Settings;
using ReMarkableRemember.Templates;

namespace ReMarkableRemember.ViewModels;

public sealed class ItemsTreeViewModel : HierarchicalTreeDataGridSource<ItemViewModel>
{
    public ItemsTreeViewModel(ISettingsService settingsService) : base(new ObservableCollection<ItemViewModel>())
    {
        this.Columns.Add(new HierarchicalExpanderColumn<ItemViewModel>(new TextColumn<ItemViewModel, String>(null, item => item.Name), item => item.Collection) { Tag = () => Language.Current.ItemsTreeHeaderName });
        this.Columns.Add(new TextColumn<ItemViewModel, String>(null, item => item.Modified.ToDisplayString(settingsService)) { Tag = () => Language.Current.ItemsTreeHeaderModified });
        this.Columns.Add(new TemplateColumn<ItemViewModel>(null, new ItemHintColumnTemplate(settingsService, item => null, item => item.CombinedHint)));
        this.Columns.Add(new TextColumn<ItemViewModel, String>(null, item => item.SyncPath) { Tag = () => Language.Current.ItemsTreeHeaderSyncPath });
        this.Columns.Add(new TemplateColumn<ItemViewModel>(null, new ItemHintColumnTemplate(settingsService, item => item.SyncDate, item => item.SyncHint)) { Tag = () => Language.Current.ItemsTreeHeaderSyncInformation });
        this.Columns.Add(new TemplateColumn<ItemViewModel>(null, new ItemHintColumnTemplate(settingsService, item => item.BackupDate, item => item.BackupHint)) { Tag = () => Language.Current.ItemsTreeHeaderBackupInformation });

        this.UpdateLocalizedHeaders();
    }

    public new ObservableCollection<ItemViewModel> Items
    {
        get { return (ObservableCollection<ItemViewModel>)base.Items; }
    }

    public new ITreeDataGridRowSelectionModel<ItemViewModel> RowSelection
    {
        get { return base.RowSelection!; }
    }

    internal void UpdateLocalizedHeaders()
    {
        foreach (IColumn<ItemViewModel> column in this.Columns)
        {
            if (column is HierarchicalExpanderColumn<ItemViewModel> hierarchicalExpanderColumn)
            {
                UpdateLocalizedHeader(hierarchicalExpanderColumn.Inner);
            }
            else
            {
                UpdateLocalizedHeader(column);
            }
        }
    }

    private static void UpdateLocalizedHeader(IColumn<ItemViewModel> column)
    {
        if (column is ColumnBase<ItemViewModel> columnBase && columnBase.Tag is Func<String> action)
        {
            columnBase.Header = action.Invoke();
        }
    }
}
