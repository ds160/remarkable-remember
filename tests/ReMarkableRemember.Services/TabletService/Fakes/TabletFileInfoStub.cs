using System;
using ReMarkableRemember.Services.TabletService.Files.Interfaces;

namespace ReMarkableRemember.Services.TabletService.Tests.Fakes;

internal sealed class TabletFileInfoStub : ITabletFileInfo
{
    public String FullName { get; init; } = String.Empty;
    public Boolean IsDirectory { get; init; }
    public Boolean IsRegularFile { get; init; }
    public String Name { get; init; } = String.Empty;

    public static TabletFileInfoStub File(String directory, String name)
    {
        return new TabletFileInfoStub
        {
            Name = name,
            FullName = directory.TrimEnd('/') + "/" + name,
            IsRegularFile = true,
            IsDirectory = false,
        };
    }

    public static TabletFileInfoStub Directory(String directory, String name)
    {
        return new TabletFileInfoStub
        {
            Name = name,
            FullName = directory.TrimEnd('/') + "/" + name,
            IsRegularFile = false,
            IsDirectory = true,
        };
    }
}
