using System;
using System.IO;
using ReMarkableRemember.DependencyInjection;

namespace ReMarkableRemember.ViewModels;

public sealed partial class ItemViewModel
{
    public partial class ItemHintViewModel
    {
        public sealed class Sync(ItemViewModel item, IServices services) : ItemHintViewModel(item, services)
        {
            protected sealed override DateTime? GetDateTime()
            {
                return this.item.SyncDate;
            }

            protected sealed override Hints GetHint()
            {
                if (this.item.Collection != null) { return Hints.None; }
                if (this.item.SyncPath == null) { return Hints.None; }
                if (this.item.DataItem == null) { return Hints.None; }

                if (this.item.DataItem.SyncPath == null && Path.Exists(this.item.SyncPath)) { return Hints.ExistsInTarget; }
                if (this.item.DataItem.SyncPath == null) { return Hints.New; }
                if (this.item.DataItem.SyncPath != this.item.SyncPath) { return Hints.SyncPathChanged; }
                if (this.item.DataItem.SyncData < this.item.Modified) { return Hints.Modified; }
                if (!Path.Exists(this.item.SyncPath)) { return Hints.NotFoundInTarget; }

                return Hints.None;
            }
        }
    }
}
