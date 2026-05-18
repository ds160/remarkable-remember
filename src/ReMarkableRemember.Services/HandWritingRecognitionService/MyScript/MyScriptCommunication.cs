using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using ReMarkableRemember.Services.HandWritingRecognitionService.Configuration;
using ReMarkableRemember.Services.HandWritingRecognitionService.MyScript.Interfaces;

namespace ReMarkableRemember.Services.HandWritingRecognitionService.MyScript;

internal sealed class MyScriptCommunication : IMyScriptCommunication
{
    private HandWritingRecognitionConfigurationMyScript? configuration;

    public void Configuration(IHandWritingRecognitionConfiguration configuration)
    {
        this.configuration = configuration as HandWritingRecognitionConfigurationMyScript;
    }

    public async Task<IMyScriptResponse> Recognize(String hmac, String jsonRequest)
    {
        if (this.configuration is null) { throw new InvalidOperationException(); }

        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("applicationKey", this.configuration.ApplicationKey);
        client.DefaultRequestHeaders.Add("hmac", hmac);
        client.DefaultRequestHeaders.Add("accept", $"{MediaTypeNames.Text.Plain}, {MediaTypeNames.Application.Json}");

        using StringContent requestContent = new StringContent(jsonRequest, Encoding.UTF8, MediaTypeNames.Application.Json);
        return new MyScriptResponse(await client.PostAsync(new Uri("https://cloud.myscript.com/api/v4.0/iink/batch"), requestContent).ConfigureAwait(false));
    }
}
