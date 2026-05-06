using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReMarkableRemember.Services.TabletService.Communication;

internal sealed class GitHubCommunication : IDisposable
{
    private readonly HttpClient httpClient;

    public GitHubCommunication()
    {
        this.httpClient = new HttpClient();
    }

    public void Dispose()
    {
        this.httpClient.Dispose();
    }

    public async Task<Byte[]> GetBytes(Uri requestUri)
    {
        return await this.httpClient.GetByteArrayAsync(requestUri).ConfigureAwait(false);
    }

    public async Task<String> GetText(Uri requestUri)
    {
        return await this.httpClient.GetStringAsync(requestUri).ConfigureAwait(false);
    }
}
