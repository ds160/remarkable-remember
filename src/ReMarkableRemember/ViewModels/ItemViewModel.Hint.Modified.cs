using System;
using ReMarkableRemember.DependencyInjection;

namespace ReMarkableRemember.ViewModels;

public sealed partial class ItemViewModel
{
    public partial class ItemHintViewModel
    {
        public sealed class Modified(ItemViewModel item, IServices services) : ItemHintViewModel(item, services)
        {
            protected sealed override DateTime? GetDateTime()
            {
                return this.item.Modified;
            }

            protected sealed override Hints GetHint()
            {
                Hints hint = this.item.BackupHint.Hint | this.item.SyncHint.Hint;

                if (this.item.Collection != null)
                {
                    foreach (ItemViewModel childItem in this.item.Collection)
                    {
                        hint |= childItem.ModifiedHint.Hint;
                    }
                }

                return hint;
            }
        }
    }
}
