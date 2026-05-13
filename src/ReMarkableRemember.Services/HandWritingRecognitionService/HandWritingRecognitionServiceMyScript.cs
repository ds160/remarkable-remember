using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Common.Notebook;
using ReMarkableRemember.Common.Notebook.Enumerations;
using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.ConfigurationService.Service;
using ReMarkableRemember.Services.HandWritingRecognitionService.Configuration;
using ReMarkableRemember.Services.HandWritingRecognitionService.Exceptions;
using ReMarkableRemember.Services.HandWritingRecognitionService.MyScript;
using ReMarkableRemember.Services.HandWritingRecognitionService.MyScript.Interfaces;

namespace ReMarkableRemember.Services.HandWritingRecognitionService;

public sealed class HandWritingRecognitionServiceMyScript : ServiceBase<HandWritingRecognitionConfigurationMyScript>, IHandWritingRecognitionService
{
    private const Int32 MAX_TASKS = 4;

    private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IMyScriptCommunication communication;

    public HandWritingRecognitionServiceMyScript(IMyScriptCommunication communication, IConfigurationService configurationService)
        : base(configurationService)
    {
        communication.Configuration(this.Configuration);

        this.communication = communication;
    }

    IHandWritingRecognitionConfiguration IHandWritingRecognitionService.Configuration
    {
        get { return this.Configuration; }
    }

    IEnumerable<String> IHandWritingRecognitionService.SupportedLanguages
    {
        get { return MyScriptLanguages.Supported; }
    }

    public async Task<String> Recognize(Notebook notebook)
    {
        String language = this.Configuration.Language;
        if (!MyScriptLanguages.Supported.Contains(language)) { throw new HandWritingRecognitionException(Language.Current.MyScriptLanguageNotSupported(language)); }

        using SemaphoreSlim throttler = new SemaphoreSlim(MAX_TASKS);

        String[] pages = await Task.WhenAll(notebook.Pages.Select(page => this.Recognize(page, language, throttler))).ConfigureAwait(false);
        return String.Join(Environment.NewLine, pages);
    }

    private async Task<String> Recognize(Page page, String language, SemaphoreSlim throttler)
    {
        await throttler.WaitAsync().ConfigureAwait(false);

        try
        {
            String jsonRequest = BuildJsonRequest(page, language);
            String hmac = this.CalculateHmac(jsonRequest);
            using IMyScriptResponse response = await this.communication.Recognize(hmac, jsonRequest).ConfigureAwait(false);

            if (response.Unauthorized)
            {
                throw new HandWritingRecognitionException(Language.Current.MyScriptAuthorizationError);
            }

            if (response.RequestTooLarge)
            {
                throw new HandWritingRecognitionException(Language.Current.MyScriptPageAnalyzeError(page.Index + 1));
            }

            return await response.Read().ConfigureAwait(false);
        }
        finally
        {
            throttler.Release();
        }
    }

    private static String BuildJsonRequest(Page page, String language)
    {
        List<Object> strokes = new List<Object>();
        foreach (Line line in page.Lines)
        {
            if (line.Type is
                not PenType.EraseArea and
                not PenType.Eraser and
                not PenType.Highlighter1 and
                not PenType.Highlighter2)
            {
                List<Double> x = new List<Double>();
                List<Double> y = new List<Double>();
                foreach (Point point in line.Points)
                {
                    x.Add(point.X);
                    y.Add(point.Y);
                }
                strokes.Add(new { PointerType = "PEN", X = x, Y = y });
            }
        }

        Object jsonRequest = new
        {
            Configuration = new { Lang = language },
            ContentType = "Text",
            StrokeGroups = new List<Object>() { new { Strokes = strokes } },
            xDPI = page.Resolution,
            yDPI = page.Resolution
        };

        return JsonSerializer.Serialize(jsonRequest, jsonSerializerOptions);
    }

    private String CalculateHmac(String jsonRequest)
    {
        using HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(this.Configuration.ApplicationKey + this.Configuration.HmacKey));
        Byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(jsonRequest));
        return String.Join(String.Empty, hashBytes.Select(hashByte => hashByte.ToString("x2", CultureInfo.InvariantCulture)));
    }
}
