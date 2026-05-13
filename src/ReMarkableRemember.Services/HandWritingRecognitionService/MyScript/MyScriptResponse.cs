using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ReMarkableRemember.Services.HandWritingRecognitionService.MyScript.Interfaces;

namespace ReMarkableRemember.Services.HandWritingRecognitionService.MyScript;

internal sealed class MyScriptResponse : IMyScriptResponse
{
    private readonly HttpResponseMessage response;

    public MyScriptResponse(HttpResponseMessage response)
    {
        this.response = response;
    }

    public Boolean RequestTooLarge { get { return this.response.StatusCode == HttpStatusCode.RequestEntityTooLarge; } }

    public Boolean Unauthorized { get { return this.response.StatusCode == HttpStatusCode.Unauthorized; } }

    public void Dispose()
    {
        this.response.Dispose();
    }

    public async Task<String> Read()
    {
        this.response.EnsureSuccessStatusCode();

        return await this.response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }
}
