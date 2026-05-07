using System;
using System.IO;
using System.Threading.Tasks;

namespace ReMarkableRemember.Services.TabletService.Communication.Interfaces;

public interface IUsbCommunication : IDisposable
{
    Task CheckConnection();

    Task<Stream> Download(String id);

    Task Upload(FileInfo file, String? parentId);
}
