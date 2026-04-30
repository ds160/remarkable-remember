using System;
using System.IO;
using ReMarkableRemember.DependencyInjection;

namespace ReMarkableRemember.ViewModels;

public sealed partial class ItemViewModel
{
    public partial class ItemHintViewModel
    {
        public sealed class Backup(ItemViewModel item, IServices services) : ItemHintViewModel(item, services)
        {
            protected sealed override DateTime? GetDateTime()
            {
                return this.item.BackupDate;
            }

            protected sealed override Hints GetHint()
            {
                if (!Path.Exists(this.services.Tablet.Configuration.Backup)) { return Hints.None; }
                if (this.item.DataItem == null) { return Hints.None; }

                if (this.item.DataItem.BackupDate == null) { return Hints.New; }
                if (this.item.DataItem.BackupDate < this.item.Modified) { return Hints.Modified; }

                return Hints.None;
            }
        }
    }
}
