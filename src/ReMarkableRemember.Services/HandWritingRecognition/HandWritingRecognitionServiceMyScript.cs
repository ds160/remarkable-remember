using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ReMarkableRemember.Common.Notebook;
using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.ConfigurationService.Service;
using ReMarkableRemember.Services.HandWritingRecognition.Configuration;
using ReMarkableRemember.Services.HandWritingRecognition.Exceptions;

namespace ReMarkableRemember.Services.HandWritingRecognition;

public sealed class HandWritingRecognitionServiceMyScript : ServiceBase<HandWritingRecognitionConfigurationMyScript>, IHandWritingRecognitionService
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private static readonly List<String> supportedLanguages = new List<String>()
    {
        "ar",
        "af_ZA",
        "sq_AL",
        "hy_AM",
        "az_AZ",
        "eu_ES",
        "be_BY",
        "bs_BA",
        "bg_BG",
        "ca_ES",
        "zh_CN",
        "zh_HK",
        "zh_TW",
        "hr_HR",
        "cs_CZ",
        "da_DK",
        "nl_BE",
        "nl_NL",
        "en_CA",
        "en_PH",
        "en_ZA",
        "en_GB",
        "en_US",
        "et_EE",
        "fa_IR",
        "fil_PH",
        "fi_FI",
        "fr_CA",
        "fr_FR",
        "ga_IE",
        "gl_ES",
        "ka_GE",
        "de_AT",
        "de_DE",
        "el_GR",
        "he_IL",
        "hi_IN",
        "hu_HU",
        "is_IS",
        "id_ID",
        "it_IT",
        "ja_JP",
        "kk_KZ",
        "ko_KR",
        "lv_LV",
        "lt_LT",
        "mk_MK",
        "ms_MY",
        "mn_MN",
        "pl_PL",
        "pt_BR",
        "pt_PT",
        "ro_RO",
        "ru_RU",
        "sk_SK",
        "sl_SI",
        "es_CO",
        "es_MX",
        "es_ES",
        "sv_SE",
        "tt_RU",
        "th_TH",
        "tr_TR",
        "uk_UA",
        "ur_PK",
        "vi_VN",
    };

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
        get { return supportedLanguages; }
    }

    public async Task<String> Recognize(Notebook.Page page, String? language = null)
    {
        language ??= this.Configuration.Language;
        if (!supportedLanguages.Contains(language)) { throw new HandWritingRecognitionException($"Language is not supported by MyScript: {language}"); }

        String requestBody = BuildRequestBody(page, language);
        String hmac = this.CalculateHmac(requestBody);

        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("applicationKey", this.Configuration.ApplicationKey);
        client.DefaultRequestHeaders.Add("hmac", hmac);
        client.DefaultRequestHeaders.Add("accept", $"{MediaTypeNames.Text.Plain}, {MediaTypeNames.Application.Json}");

        using StringContent requestContent = new StringContent(requestBody, Encoding.UTF8, MediaTypeNames.Application.Json);
        HttpResponseMessage response = await client.PostAsync(new Uri("https://cloud.myscript.com/api/v4.0/iink/batch"), requestContent).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new HandWritingRecognitionException("MyScript authorization information not configured or wrong.");
        }

        if (response.StatusCode == HttpStatusCode.RequestEntityTooLarge)
        {
            throw new HandWritingRecognitionException($"MyScript cannot analyze page {page.Index + 1}, it has to much content.");
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    private static String BuildRequestBody(Notebook.Page page, String language)
    {
        List<BatchInput.Stroke> strokes = new List<BatchInput.Stroke>();
        foreach (Notebook.Page.Line line in page.Lines)
        {
            if (line.Type is
                not Notebook.Page.Line.PenType.EraseArea and
                not Notebook.Page.Line.PenType.Eraser and
                not Notebook.Page.Line.PenType.Highlighter1 and
                not Notebook.Page.Line.PenType.Highlighter2)
            {
                List<Double> x = new List<Double>();
                List<Double> y = new List<Double>();
                foreach (Notebook.Page.Line.Point point in line.Points)
                {
                    x.Add(point.X);
                    y.Add(point.Y);
                }
                strokes.Add(new BatchInput.Stroke(x, y));
            }
        }

        BatchInput batchInput = new BatchInput(page, language, strokes);
        return JsonSerializer.Serialize(batchInput, jsonSerializerOptions);
    }

    private String CalculateHmac(String requestBody)
    {
        using HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(this.Configuration.ApplicationKey + this.Configuration.HmacKey));
        Byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(requestBody));
        return String.Join(String.Empty, hashBytes.Select(hashByte => hashByte.ToString("x2", CultureInfo.InvariantCulture)));
    }

    private sealed class BatchInput
    {
        public BatchInput(Notebook.Page page, String language, List<Stroke> strokes)
        {
            this.Configuration = new { Lang = language };
            this.ContentType = "Text";
            this.XDPI = page.Resolution;
            this.YDPI = page.Resolution;

            this.StrokeGroups = new List<Object>() { new { Strokes = strokes } };
        }

        public Object Configuration { get; }
        public String ContentType { get; }
        public IEnumerable<Object> StrokeGroups { get; }
        [JsonPropertyName("xDPI")]
        public Int32 XDPI { get; }
        [JsonPropertyName("yDPI")]
        public Int32 YDPI { get; }

        internal sealed class Stroke
        {
            public Stroke(List<Double> x, List<Double> y)
            {
                this.PointerType = "PEN";
                this.X = x;
                this.Y = y;
            }

            public String PointerType { get; }
            public IEnumerable<Double> X { get; }
            public IEnumerable<Double> Y { get; }
        }
    }
}
