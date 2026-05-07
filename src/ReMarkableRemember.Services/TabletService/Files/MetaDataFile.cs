using System;
using ReMarkableRemember.Services.TabletService.Files.Interfaces;

namespace ReMarkableRemember.Services.TabletService.Files;

internal struct MetaDataFile : ITabletFile
{
    public Boolean? Deleted { get; set; }
    public String LastModified { get; set; }
    public String Parent { get; set; }
    public String Type { get; set; }
    public String VisibleName { get; set; }
}
