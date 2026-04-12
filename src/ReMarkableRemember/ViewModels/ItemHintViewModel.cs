using System;
using System.IO;
using System.Reflection;
using Avalonia.Svg;
using ReactiveUI;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Helper;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.Settings;

namespace ReMarkableRemember.ViewModels;

public abstract class ItemHintViewModel : ViewModelBase
{
    public sealed class Backup(ItemViewModel item, ISettingsService settingsService, ITabletService tabletService) : ItemHintViewModel(item, settingsService)
    {
        protected sealed override DateTime? GetDateTime(ItemViewModel item)
        {
            return item.BackupDate;
        }

        protected sealed override Hints GetHint(ItemViewModel item)
        {
            if (!Path.Exists(tabletService.Configuration.Backup)) { return Hints.None; }
            if (item.DataItem == null) { return Hints.None; }

            if (item.DataItem.BackupDate == null) { return Hints.New; }
            if (item.DataItem.BackupDate < item.Modified) { return Hints.Modified; }

            return Hints.None;
        }
    }

    public sealed class Combined(ItemViewModel item, ISettingsService settingsService) : ItemHintViewModel(item, settingsService)
    {
        protected override DateTime? GetDateTime(ItemViewModel item)
        {
            return null;
        }

        protected override Hints GetHint(ItemViewModel item)
        {
            Hints hint = item.BackupHint.Hint | item.SyncHint.Hint;

            if (item.Collection != null)
            {
                foreach (ItemViewModel childItem in item.Collection)
                {
                    hint |= this.GetHint(childItem);
                }
            }

            return hint;
        }
    }

    public sealed class Sync(ItemViewModel item, ISettingsService settingsService) : ItemHintViewModel(item, settingsService)
    {
        protected override DateTime? GetDateTime(ItemViewModel item)
        {
            return item.SyncDate;
        }

        protected override Hints GetHint(ItemViewModel item)
        {
            if (item.Collection != null) { return Hints.None; }
            if (item.SyncPath == null) { return Hints.None; }
            if (item.DataItem == null) { return Hints.None; }

            if (item.DataItem.SyncPath == null && Path.Exists(item.SyncPath)) { return Hints.ExistsInTarget; }
            if (item.DataItem.SyncPath == null) { return Hints.New; }
            if (item.DataItem.SyncPath != item.SyncPath) { return Hints.SyncPathChanged; }
            if (item.DataItem.SyncData < item.Modified) { return Hints.Modified; }
            if (!Path.Exists(item.SyncPath)) { return Hints.NotFoundInTarget; }

            return Hints.None;
        }
    }

    [Flags]
    public enum Hints
    {
        None = 0x00,
        NotFoundInTarget = 0x01,
        SyncPathChanged = 0x02,
        Modified = 0x04,
        New = 0x08,
        ExistsInTarget = 0x10
    }

    private static readonly SvgImage imageRed;
    private static readonly SvgImage imageGreen;
    private static readonly SvgImage imageYellow;

    private readonly ItemViewModel item;
    private readonly ISettingsService settingsService;

    static ItemHintViewModel()
    {
        imageRed = ImageLoader.Svg("Dots/Red.svg");
        imageGreen = ImageLoader.Svg("Dots/Green.svg");
        imageYellow = ImageLoader.Svg("Dots/Yellow.svg");
    }

    protected ItemHintViewModel(ItemViewModel item, ISettingsService settingsService)
    {
        this.item = item;
        this.settingsService = settingsService;
    }

    public String? DateTime { get { return this.GetDateTime(this.item)?.ToDisplayString(this.settingsService); } }

    internal Hints Hint { get { return this.GetHint(this.item); } }

    public SvgImage? Image { get { return this.GetImage(); } }

    public String? ToolTip { get { return this.GetToolTip(); } }

    protected abstract DateTime? GetDateTime(ItemViewModel item);

    protected abstract Hints GetHint(ItemViewModel item);

    private SvgImage? GetImage()
    {
        if (this.Hint.HasFlag(Hints.ExistsInTarget)) { return imageRed; }
        if (this.Hint.HasFlag(Hints.New)) { return imageYellow; }
        if (this.Hint.HasFlag(Hints.Modified)) { return imageYellow; }
        if (this.Hint.HasFlag(Hints.SyncPathChanged)) { return imageYellow; }
        if (this.Hint.HasFlag(Hints.NotFoundInTarget)) { return imageYellow; }

        if (this.Hint == Hints.None) { return (this.DateTime != null) ? imageGreen : null; }

        throw new NotImplementedException();
    }

    private String? GetToolTip()
    {
        if (this.Hint.HasFlag(Hints.ExistsInTarget)) { return Language.Current.ItemHintExistsInTarget; }
        if (this.Hint.HasFlag(Hints.New)) { return Language.Current.ItemHintNew; }
        if (this.Hint.HasFlag(Hints.Modified)) { return Language.Current.ItemHintModified; }
        if (this.Hint.HasFlag(Hints.SyncPathChanged)) { return Language.Current.ItemHintSyncPathChanged; }
        if (this.Hint.HasFlag(Hints.NotFoundInTarget)) { return Language.Current.ItemHintNotFoundInTarget; }

        if (this.Hint == Hints.None) { return (this.DateTime != null) ? Language.Current.ItemHintUpToDate : null; }

        throw new NotImplementedException();
    }

    internal void RaiseChanged()
    {
        foreach (PropertyInfo property in this.GetType().GetProperties())
        {
            this.RaisePropertyChanged(property.Name);
        }
    }
}
