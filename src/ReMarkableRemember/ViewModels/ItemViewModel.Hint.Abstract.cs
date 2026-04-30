using System;
using System.Reflection;
using Avalonia.Media;
using ReactiveUI;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.DependencyInjection;
using ReMarkableRemember.Helper;

namespace ReMarkableRemember.ViewModels;

public partial class ItemViewModel
{
    public abstract partial class ItemHintViewModel : ViewModelBase
    {
        [Flags]
        protected internal enum Hints
        {
            None = 0x00,
            NotFoundInTarget = 0x01,
            SyncPathChanged = 0x02,
            Modified = 0x04,
            New = 0x08,
            ExistsInTarget = 0x10
        }

        private const String IMAGE_GREEN = "Dots/Green.svg";
        private const String IMAGE_RED = "Dots/Red.svg";
        private const String IMAGE_YELLOW = "Dots/Yellow.svg";

        private readonly ItemViewModel item;
        private readonly IServices services;

        protected ItemHintViewModel(ItemViewModel item, IServices services)
        {
            this.item = item;
            this.services = services;
        }

        public String? DateTime { get { return this.GetDateTime()?.ToDisplayString(this.services.Settings); } }

        internal Hints Hint { get { return this.GetHint(); } }

        public IImage? Image { get { return this.GetImagePath() is String image ? ImageLoader.Svg(image) : null; } }

        public String? ToolTip { get { return this.GetToolTip(); } }

        protected abstract DateTime? GetDateTime();

        protected abstract Hints GetHint();

        private String? GetImagePath()
        {
            Hints hint = this.Hint;

            if (hint.HasFlag(Hints.ExistsInTarget)) { return IMAGE_RED; }
            if (hint.HasFlag(Hints.New)) { return IMAGE_YELLOW; }
            if (hint.HasFlag(Hints.Modified)) { return IMAGE_YELLOW; }
            if (hint.HasFlag(Hints.SyncPathChanged)) { return IMAGE_YELLOW; }
            if (hint.HasFlag(Hints.NotFoundInTarget)) { return IMAGE_YELLOW; }

            if (hint == Hints.None) { return (this.DateTime != null) ? IMAGE_GREEN : null; }

            throw new NotImplementedException();
        }

        private String? GetToolTip()
        {
            Hints hint = this.Hint;

            if (hint.HasFlag(Hints.ExistsInTarget)) { return Language.Current.ItemHintExistsInTarget; }
            if (hint.HasFlag(Hints.New)) { return Language.Current.ItemHintNew; }
            if (hint.HasFlag(Hints.Modified)) { return Language.Current.ItemHintModified; }
            if (hint.HasFlag(Hints.SyncPathChanged)) { return Language.Current.ItemHintSyncPathChanged; }
            if (hint.HasFlag(Hints.NotFoundInTarget)) { return Language.Current.ItemHintNotFoundInTarget; }

            if (hint == Hints.None) { return (this.DateTime != null) ? Language.Current.ItemHintUpToDate : null; }

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
}
