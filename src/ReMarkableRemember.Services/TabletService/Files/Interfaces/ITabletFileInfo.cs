using System;

namespace ReMarkableRemember.Services.TabletService.Files.Interfaces;

internal interface ITabletFileInfo
{
    String FullName { get; }
    Boolean IsDirectory { get; }
    Boolean IsRegularFile { get; }
    String Name { get; }
}
