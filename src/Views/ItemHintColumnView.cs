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

public sealed class ItemHintColumnView : StackPanel
{
    private static readonly String? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    private readonly Func<DateTime?> dateTime;
    private readonly Func<ItemViewModel.Hint> hint;
    private readonly Image image;
    private readonly TextBlock textBlock;

    internal ItemHintColumnView(ItemViewModel item, Func<ItemViewModel, DateTime?> dateTime, Func<ItemViewModel, ItemViewModel.Hint> hint)
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

        ToolTip.SetTip(this, ItemViewModel.GetToolTip(dateTime, hint));
    }

    private static Bitmap? GetImage(DateTime? dateTime, ItemViewModel.Hint hint)
    {
        ItemViewModel.Image image = ItemViewModel.GetImage(dateTime, hint);
        return (image != ItemViewModel.Image.None) ? new Bitmap(AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/Dot{image}.png"))) : null;
    }
}
