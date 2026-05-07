using System;
using ReMarkableRemember.Services.TabletService.Files.Interfaces;
using Renci.SshNet.Sftp;

namespace ReMarkableRemember.Services.TabletService.Files;

internal sealed class TabletFile : ITabletFile
{
    public TabletFile(ISftpFile file)
    {
        this.FullName = file.FullName;
        this.IsDirectory = file.IsDirectory;
        this.IsRegularFile = file.IsRegularFile;
        this.Name = file.Name;
    }

    public String FullName { get; }
    public Boolean IsDirectory { get; }
    public Boolean IsRegularFile { get; }
    public String Name { get; }
}
