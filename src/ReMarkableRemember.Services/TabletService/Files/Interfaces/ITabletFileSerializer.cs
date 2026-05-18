using System;

namespace ReMarkableRemember.Services.TabletService.Files.Interfaces;

internal interface ITabletFileSerializer
{
    T Deserialize<T>(String fileText) where T : struct, ITabletFile;

    String Serialize<T>(T value) where T : struct, ITabletFile;
}
