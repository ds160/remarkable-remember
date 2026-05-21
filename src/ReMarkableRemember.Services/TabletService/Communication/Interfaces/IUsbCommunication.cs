using System;
using System.IO;
using System.Threading.Tasks;

namespace ReMarkableRemember.Services.TabletService.Communication.Interfaces;

internal interface IUsbCommunication : IDisposable
{
    Task<Stream> Download(String id);

    Task Upload(FileInfo file, String? parentId);
}
