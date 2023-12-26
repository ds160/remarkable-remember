using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReMarkableRemember.Models;

internal sealed class MyScript
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly Settings settings;

    public MyScript(Settings settings)
    {
        this.settings = settings;
    }

    public async Task<String> Recognize(Notebook.Page page, String language)
    {
        String requestBody = BuildRequestBody(page, language);
        String hmac = this.CalculateHmac(requestBody);

        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("applicationKey", this.settings.MyScriptApplicationKey);
        client.DefaultRequestHeaders.Add("hmac", hmac);
        client.DefaultRequestHeaders.Add("accept", "text/plain, application/json");

        using StringContent requestContent = new StringContent(requestBody, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(new Uri("https://cloud.myscript.com/api/v4.0/iink/batch"), requestContent).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new MyScriptException("MyScript authorization information not configured or wrong.");
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

        BatchInput batchInput = new BatchInput(language, strokes);
        return JsonSerializer.Serialize(batchInput, jsonSerializerOptions);
    }

    private String CalculateHmac(String requestBody)
    {
        using HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(this.settings.MyScriptApplicationKey + this.settings.MyScriptHmacKey));
        Byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(requestBody));
        return String.Join(String.Empty, hashBytes.Select(hashByte => hashByte.ToString("x2", CultureInfo.InvariantCulture)));
    }

    private sealed class BatchInput
    {
        public BatchInput(String lang, List<Stroke> strokes)
        {
            this.Configuration = new { Lang = lang };
            this.ContentType = "Text";
            this.Height = 1872;
            this.Width = 1404;
            this.XDPI = 226;
            this.YDPI = 226;

            this.StrokeGroups = new List<Object>() { new { Strokes = strokes } };
        }

        public Object Configuration { get; }
        public String ContentType { get; }
        public Int32 Height { get; }
        public IEnumerable<Object> StrokeGroups { get; }
        public Int32 Width { get; }
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
