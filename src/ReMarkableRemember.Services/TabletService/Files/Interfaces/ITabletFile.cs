using System;

namespace ReMarkableRemember.Services.TabletService.Files.Interfaces;

public interface ITabletFile
{
    String FullName { get; }
    Boolean IsDirectory { get; }
    Boolean IsRegularFile { get; }
    String Name { get; }
}
