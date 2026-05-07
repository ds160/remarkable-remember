using System;

namespace ReMarkableRemember.Services.TabletService.Files.Interfaces;

public interface ITabletFileInfo
{
    String FullName { get; }
    Boolean IsDirectory { get; }
    Boolean IsRegularFile { get; }
    String Name { get; }
}
