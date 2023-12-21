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
    private readonly Func<Item, Item.Hint?> getHint;

    public TreeDataGridItemHintColumn(Func<Item, DateTime?> getDateTime, Func<Item, Item.Hint?> getHint)
    {
        this.getDateTime = getDateTime;
        this.getHint = getHint;
    }
    public Control? Build(Object? param)
    {
        Item item = param as Item ?? throw new ArgumentNullException(nameof(param));

        DateTime? dateTime = this.getDateTime(item);
        Item.Hint? hint = this.getHint(item);

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

    private static Bitmap? GetImage(DateTime? dateTime, Item.Hint? hint)
    {
        if (hint == null) { return (dateTime != null) ? new Bitmap("Assets/DotGreen.png") : null; }

        switch (hint)
        {
            case Item.Hint.DocumentSyncPathChanged: return new Bitmap("Assets/DotYellow.png");
            case Item.Hint.DocumentExistsInTarget: return new Bitmap("Assets/DotRed.png");
            case Item.Hint.DocumentNotFoundInTarget: return new Bitmap("Assets/DotYellow.png");
            case Item.Hint.ItemModified: return new Bitmap("Assets/DotYellow.png");
            case Item.Hint.ItemNew: return new Bitmap("Assets/DotYellow.png");
            case Item.Hint.ItemTrashed: return new Bitmap("Assets/DotRed.png");
            default: throw new NotImplementedException();
        }
    }
    private static String? GetToolTip(DateTime? dateTime, Item.Hint? hint)
    {
        if (hint == null) { return (dateTime != null) ? "Up-to-date" : null; }

        switch (hint)
        {
            case Item.Hint.DocumentSyncPathChanged: return "Document sync path changed";
            case Item.Hint.DocumentExistsInTarget: return "Document exists already in target directory";
            case Item.Hint.DocumentNotFoundInTarget: return "Document not found in target directory";
            case Item.Hint.ItemModified: return "Modified item";
            case Item.Hint.ItemNew: return "New item";
            case Item.Hint.ItemTrashed: return "Trashed item";
            default: throw new NotImplementedException();
        }
    }
}
