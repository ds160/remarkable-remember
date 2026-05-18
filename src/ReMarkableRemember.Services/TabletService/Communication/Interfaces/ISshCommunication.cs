using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReMarkableRemember.Services.TabletService.Files.Interfaces;

namespace ReMarkableRemember.Services.TabletService.Communication.Interfaces;

internal interface ISshCommunication : IDisposable
{
    Task Connect();

    Task Execute(String command, Boolean checkResult = true);

    Task FileDelete(String path);

    Task FileDownload(String path, String targetPath);

    Task<IEnumerable<ITabletFileInfo>> FileList(String directoryPath);

    Task<Byte[]> FileReadBytes(String path);

    Task<String> FileReadText(String path);

    Task FileWrite(String path, Object content, Boolean contentRequired = true);
}
