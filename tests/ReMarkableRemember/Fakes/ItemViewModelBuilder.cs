using System;
using ReMarkableRemember.DependencyInjection;
using ReMarkableRemember.Services.DataService.Models;
using ReMarkableRemember.Services.TabletService.Models;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Tests.Fakes;

internal static class ItemViewModelBuilder
{
    public static ItemViewModel Create(
        TabletItem tabletItem,
        IServices services,
        ItemViewModel? parent = null,
        ItemData? dataItem = null,
        String? syncPath = null)
    {
        ItemViewModel vm = new ItemViewModel(tabletItem, parent, services);
        vm.SetForTesting(dataItem, syncPath);
        return vm;
    }

    public static TabletItem MakeDocument(String id = "doc-1", String name = "Doc.pdf", DateTime? modified = null, String parent = "")
    {
        DateTime mod = modified ?? new DateTime(2026, 5, 18, 12, 0, 0, DateTimeKind.Utc);
        Int64 ms = (Int64)(mod - DateTime.UnixEpoch).TotalMilliseconds;
        return new TabletItem(id, ms.ToString(System.Globalization.CultureInfo.InvariantCulture), parent, "DocumentType", name);
    }

    public static TabletItem MakeCollection(String id = "coll-1", String name = "Folder", String parent = "")
    {
        return new TabletItem(id, "1700000000000", parent, "CollectionType", name);
    }
}
