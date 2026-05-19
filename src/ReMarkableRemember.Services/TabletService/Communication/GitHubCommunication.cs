using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ReMarkableRemember.Services.TabletService.Communication.Interfaces;

namespace ReMarkableRemember.Services.TabletService.Communication;

internal sealed class GitHubCommunication : CommunicationBase, IGitHubCommunication
{
    private readonly HttpClient httpClient;

    public GitHubCommunication(SemaphoreSlim semaphore)
        : base(semaphore)
    {
        this.httpClient = new HttpClient() { BaseAddress = new Uri("https://raw.githubusercontent.com") };
    }

    public async Task<Byte[]> GetLamyEraserBinary()
    {
        return await this.httpClient.GetByteArrayAsync("/isaacwisdom/RemarkableLamyEraser/v1/RemarkableLamyEraser/RemarkableLamyEraser").ConfigureAwait(false);
    }

    public async Task<String> GetLamyEraserService()
    {
        return await this.httpClient.GetStringAsync("/isaacwisdom/RemarkableLamyEraser/v1/RemarkableLamyEraser/LamyEraser.service").ConfigureAwait(false);
    }

    protected sealed override void OnDisposing()
    {
        this.httpClient.Dispose();
    }
}
