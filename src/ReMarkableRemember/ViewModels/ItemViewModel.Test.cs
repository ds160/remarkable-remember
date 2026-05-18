using System;
using ReMarkableRemember.Services.DataService.Models;

namespace ReMarkableRemember.ViewModels;

public partial class ItemViewModel
{
    internal void SetForTesting(ItemData? dataItem, String? syncPath)
    {
        this.DataItem = dataItem;
        this.SyncPath = syncPath;
    }
}
