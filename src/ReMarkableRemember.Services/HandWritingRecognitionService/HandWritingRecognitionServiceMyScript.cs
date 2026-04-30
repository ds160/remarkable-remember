using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Common.Notebook;
using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.ConfigurationService.Service;
using ReMarkableRemember.Services.HandWritingRecognitionService.Configuration;
using ReMarkableRemember.Services.HandWritingRecognitionService.Exceptions;

namespace ReMarkableRemember.Services.HandWritingRecognitionService;

public sealed partial class HandWritingRecognitionServiceMyScript : ServiceBase<HandWritingRecognitionConfigurationMyScript>, IHandWritingRecognitionService
{
    private const Int32 MAX_TASKS = 4;

    public HandWritingRecognitionServiceMyScript(IConfigurationService configurationService)
        : base(configurationService)
    {
    }

    IHandWritingRecognitionConfiguration IHandWritingRecognitionService.Configuration
    {
        get { return this.Configuration; }
    }

    IEnumerable<String> IHandWritingRecognitionService.SupportedLanguages
    {
        get { return languages; }
    }

    public async Task<IEnumerable<String>> Recognize(Notebook notebook)
    {
        String language = this.Configuration.Language;
        if (!languages.Contains(language)) { throw new HandWritingRecognitionException(Language.Current.MyScriptLanguageNotSupported(language)); }

        using SemaphoreSlim throttler = new SemaphoreSlim(MAX_TASKS);

        return await Task.WhenAll(notebook.Pages.Select(page => this.Recognize(page, language, throttler))).ConfigureAwait(false);
    }

    private async Task<String> Recognize(Page page, String language, SemaphoreSlim throttler)
    {
        await throttler.WaitAsync().ConfigureAwait(false);

        try
        {
            String jsonRequest = BuildJsonRequest(page, language);
            String hmac = this.CalculateHmac(jsonRequest);

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("applicationKey", this.Configuration.ApplicationKey);
            client.DefaultRequestHeaders.Add("hmac", hmac);
            client.DefaultRequestHeaders.Add("accept", $"{MediaTypeNames.Text.Plain}, {MediaTypeNames.Application.Json}");

            using StringContent requestContent = new StringContent(jsonRequest, Encoding.UTF8, MediaTypeNames.Application.Json);
            HttpResponseMessage response = await client.PostAsync(new Uri("https://cloud.myscript.com/api/v4.0/iink/batch"), requestContent).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new HandWritingRecognitionException(Language.Current.MyScriptAuthorizationError);
            }

            if (response.StatusCode == HttpStatusCode.RequestEntityTooLarge)
            {
                throw new HandWritingRecognitionException(Language.Current.MyScriptPageAnalyzeError(page.Index + 1));
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
        finally
        {
            throttler.Release();
        }
    }

    private String CalculateHmac(String jsonRequest)
    {
        using HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(this.Configuration.ApplicationKey + this.Configuration.HmacKey));
        Byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(jsonRequest));
        return String.Join(String.Empty, hashBytes.Select(hashByte => hashByte.ToString("x2", CultureInfo.InvariantCulture)));
    }
}
