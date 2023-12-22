using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReMarkableRemember.Helper;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Views;

internal sealed class ItemHintColumnView : StackPanel
{
    private static readonly String? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    private readonly Func<DateTime?> dateTime;
    private readonly Func<ItemViewModel.Hint> hint;
    private readonly Image image;
    private readonly TextBlock textBlock;

    public ItemHintColumnView(ItemViewModel item, Func<ItemViewModel, DateTime?> dateTime, Func<ItemViewModel, ItemViewModel.Hint> hint)
    {
        this.dateTime = () => dateTime(item);
        this.hint = () => hint(item);
        this.image = new Image() { Height = 10.0, Width = 10.0 };
        this.textBlock = new TextBlock();

        this.Children.Add(this.image);
        this.Children.Add(this.textBlock);
        this.Margin = new Thickness(4.0);
        this.Orientation = Orientation.Horizontal;
        this.Spacing = 4.0;

        item.PropertyChanged += (s, e) => this.Update();

        this.Update();
    }

    private void Update()
    {
        DateTime? dateTime = this.dateTime();
        ItemViewModel.Hint hint = this.hint();

        this.image.Source = GetImage(dateTime, hint);
        this.textBlock.Text = dateTime?.ToDisplayString();

        ToolTip.SetTip(this, GetToolTip(dateTime, hint));
    }

    private static Bitmap? GetImage(DateTime? dateTime, ItemViewModel.Hint hint)
    {
        if ((hint & ItemViewModel.Hint.ExistsInTarget) != 0) { return LoadBitmap("/Assets/DotRed.png"); }
        if ((hint & ItemViewModel.Hint.New) != 0) { return LoadBitmap("/Assets/DotYellow.png"); }
        if ((hint & ItemViewModel.Hint.Modified) != 0) { return LoadBitmap("/Assets/DotYellow.png"); }
        if ((hint & ItemViewModel.Hint.SyncPathChanged) != 0) { return LoadBitmap("/Assets/DotYellow.png"); }
        if ((hint & ItemViewModel.Hint.NotFoundInTarget) != 0) { return LoadBitmap("/Assets/DotYellow.png"); }

        if (hint == 0) { return (dateTime != null) ? LoadBitmap("/Assets/DotGreen.png") : null; }

        throw new NotImplementedException();
    }

    private static String? GetToolTip(DateTime? dateTime, ItemViewModel.Hint hint)
    {
        if ((hint & ItemViewModel.Hint.ExistsInTarget) != 0) { return "Exists already in target directory"; }
        if ((hint & ItemViewModel.Hint.New) != 0) { return "New"; }
        if ((hint & ItemViewModel.Hint.Modified) != 0) { return "Modified"; }
        if ((hint & ItemViewModel.Hint.SyncPathChanged) != 0) { return "Sync path changed"; }
        if ((hint & ItemViewModel.Hint.NotFoundInTarget) != 0) { return "Not found in target directory"; }

        if (hint == 0) { return (dateTime != null) ? "Up-to-date" : null; }

        throw new NotImplementedException();
    }

    private static Bitmap LoadBitmap(String uri)
    {
        return new Bitmap(AssetLoader.Open(new Uri($"avares://{assemblyName}{uri}")));
    }
}
