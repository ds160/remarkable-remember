using System;
using System.Collections.Generic;
using System.Globalization;

namespace ReMarkableRemember.Services.TabletService.Models;

public sealed class TabletItem
{
    internal TabletItem(String id, String lastModified, String parent, String type, String visibleName)
    {
        this.Collection = type == "CollectionType" ? new List<TabletItem>() : null;
        this.Id = id;
        this.Modified = DateTime.UnixEpoch.AddMilliseconds(Double.Parse(lastModified, CultureInfo.InvariantCulture));
        this.Name = type == "DocumentType" && !visibleName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? $"{visibleName}.pdf" : visibleName;
        this.ParentCollectionId = parent;
        this.Trashed = parent == "trash";
    }

    public List<TabletItem>? Collection { get; }

    public String Id { get; }

    public DateTime Modified { get; }

    public String Name { get; }

    public String ParentCollectionId { get; }

    public Boolean Trashed { get; set; }
}
