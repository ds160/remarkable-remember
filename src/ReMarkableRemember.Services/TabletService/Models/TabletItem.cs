using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ReMarkableRemember.Services.TabletService.Models;

public sealed class TabletItem
{
    internal TabletItem(String id, String lastModified, String parent, String type, String visibleName)
    {
        String name = visibleName;
        foreach (Char invalidChar in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(invalidChar, ' ');
        }

        this.Collection = type == "CollectionType" ? new List<TabletItem>() : null;
        this.Id = id;
        this.Modified = DateTime.UnixEpoch.AddMilliseconds(Double.Parse(lastModified[..Math.Min(lastModified.Length, 13)], CultureInfo.InvariantCulture));
        this.Name = type == "DocumentType" && !name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? $"{name}.pdf" : name;
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
