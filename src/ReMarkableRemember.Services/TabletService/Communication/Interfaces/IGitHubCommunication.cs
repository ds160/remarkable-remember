using System;
using System.Threading.Tasks;

namespace ReMarkableRemember.Services.TabletService.Communication.Interfaces;

public interface IGitHubCommunication : IDisposable
{
    Task<Byte[]> GetLamyEraserBinary();

    Task<String> GetLamyEraserService();
}
