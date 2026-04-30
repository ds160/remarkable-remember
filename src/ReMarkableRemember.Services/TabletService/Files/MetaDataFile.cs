using System;

namespace ReMarkableRemember.Services.TabletService.Files;

internal struct MetaDataFile
{
    public Boolean? Deleted { get; set; }
    public String LastModified { get; set; }
    public String Parent { get; set; }
    public String Type { get; set; }
    public String VisibleName { get; set; }
}
