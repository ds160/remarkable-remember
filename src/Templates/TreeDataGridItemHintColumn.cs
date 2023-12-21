using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using ReMarkableRemember.Helper;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.Templates;

internal sealed class TreeDataGridItemHintColumn : IDataTemplate
{
    private readonly Func<Item, DateTime?> getDateTime;
    private readonly Func<Item, Item.Hint> getHint;

    public TreeDataGridItemHintColumn(Func<Item, DateTime?> getDateTime, Func<Item, Item.Hint> getHint)
    {
        this.getDateTime = getDateTime;
        this.getHint = getHint;
    }
    public Control? Build(Object? param)
    {
        Item item = param as Item ?? throw new ArgumentNullException(nameof(param));

        DateTime? dateTime = this.getDateTime(item);
        Item.Hint hint = this.getHint(item);

        StackPanel stackPanel = new StackPanel() { Margin = new Thickness(4.0), Orientation = Orientation.Horizontal, Spacing = 4.0 };
        stackPanel.Children.Add(new Image() { Source = GetImage(dateTime, hint), Height = 10.0, Width = 10.0 });
        stackPanel.Children.Add(new TextBlock() { Text = dateTime?.ToDisplayString() });
        ToolTip.SetTip(stackPanel, GetToolTip(dateTime, hint));
        return stackPanel;
    }

    public Boolean Match(Object? data)
    {
        return data is Item;
    }

    public static Item.Hint GetHintIncludingCollection(Item item)
    {
        Item.Hint hint = item.BackupHint | item.SyncHint;

        if (item.Collection != null)
        {
            foreach (Item childItem in item.Collection)
            {
                hint |= GetHintIncludingCollection(childItem);
            }
        }

        return hint;
    }

    private static Bitmap? GetImage(DateTime? dateTime, Item.Hint hint)
    {
        if ((hint & Item.Hint.Trashed) != 0) { return new Bitmap("Assets/DotRed.png"); }
        if ((hint & Item.Hint.ExistsInTarget) != 0) { return new Bitmap("Assets/DotRed.png"); }

        if ((hint & Item.Hint.New) != 0) { return new Bitmap("Assets/DotYellow.png"); }
        if ((hint & Item.Hint.Modified) != 0) { return new Bitmap("Assets/DotYellow.png"); }
        if ((hint & Item.Hint.SyncPathChanged) != 0) { return new Bitmap("Assets/DotYellow.png"); }
        if ((hint & Item.Hint.NotFoundInTarget) != 0) { return new Bitmap("Assets/DotYellow.png"); }

        if (hint == 0) { return (dateTime != null) ? new Bitmap("Assets/DotGreen.png") : null; }

        throw new NotImplementedException();
    }
    private static String? GetToolTip(DateTime? dateTime, Item.Hint hint)
    {
        if ((hint & Item.Hint.Trashed) != 0) { return "Trashed"; }
        if ((hint & Item.Hint.ExistsInTarget) != 0) { return "Exists already in target directory"; }

        if ((hint & Item.Hint.New) != 0) { return "New"; }
        if ((hint & Item.Hint.Modified) != 0) { return "Modified"; }
        if ((hint & Item.Hint.SyncPathChanged) != 0) { return "Sync path changed"; }
        if ((hint & Item.Hint.NotFoundInTarget) != 0) { return "Not found in target directory"; }

        if (hint == 0) { return (dateTime != null) ? "Up-to-date" : null; }

        throw new NotImplementedException();
    }
}
