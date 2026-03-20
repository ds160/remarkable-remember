using System;
using System.Collections.Generic;

namespace ReMarkableRemember.Services.TabletService.Models;

public sealed class TabletItems
{
    private readonly IReadOnlyDictionary<String, Exception> notReadable;

    public TabletItems(IEnumerable<TabletItem> items, IReadOnlyDictionary<String, Exception> notReadable)
    {
        this.notReadable = notReadable;

        this.Items = items;
    }

    public IEnumerable<TabletItem> Items { get; }

    public IEnumerable<String> NotReadable { get { return this.notReadable.Keys; } }
}
