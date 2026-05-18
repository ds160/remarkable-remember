using System;
using System.Threading.Tasks;

namespace ReMarkableRemember.Services.TabletService.Communication.Interfaces;

internal interface IGitHubCommunication : IDisposable
{
    Task<Byte[]> GetLamyEraserBinary();

    Task<String> GetLamyEraserService();
}
