using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReMarkableRemember.Models;

public class MyScriptBatchInput
{
    public MyScriptBatchInput(String lang, MyScriptStrokeGroup strokeGroup)
    {
        this.Configuration = new MyScriptConfiguration(lang);
        this.ContentType = "Text";
        this.Height = 1872;
        this.Width = 1404;
        this.XDPI = 226;
        this.YDPI = 226;

        this.StrokeGroups = new Collection<MyScriptStrokeGroup>() { strokeGroup };
    }

    public MyScriptConfiguration Configuration { get; }
    public String ContentType { get; }
    public Int32 Height { get; }
    public Collection<MyScriptStrokeGroup> StrokeGroups { get; }
    public Int32 Width { get; }
    public Int32 XDPI { get; }
    public Int32 YDPI { get; }
}

public class MyScriptConfiguration
{
    public MyScriptConfiguration(String lang)
    {
        this.Lang = lang;
    }

    public String Lang { get; }
}

public class MyScriptStrokeGroup
{
    public MyScriptStrokeGroup()
    {
        this.Strokes = new Collection<MyScriptStroke>();
    }

    public Collection<MyScriptStroke> Strokes { get; }
}

public class MyScriptStroke
{
    public MyScriptStroke()
    {
        this.PointerType = "PEN";
        this.X = new Collection<Double>();
        this.Y = new Collection<Double>();
    }

    public String PointerType { get; }
    public Collection<Double> X { get; }
    public Collection<Double> Y { get; }
}

public sealed class MyScript
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private String? applicationKey;
    private String? hmacKey;

    public MyScript(String? applicationKey, String? hmacKey)
    {
        this.applicationKey = applicationKey;
        this.hmacKey = hmacKey;
    }

    public async Task<String> Recognize(Notebook notebook, String language)
    {
        if (notebook == null) { throw new ArgumentNullException(nameof(notebook)); }

        String requestBody = BuildRequestBody(notebook, language);
        String hmac = this.CalculateHmac(requestBody);

        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("applicationKey", this.applicationKey);
        client.DefaultRequestHeaders.Add("hmac", hmac);
        client.DefaultRequestHeaders.Add("accept", "text/plain, application/json");

        using StringContent requestContent = new StringContent(requestBody, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(new Uri("https://cloud.myscript.com/api/v4.0/iink/batch"), requestContent).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    public void Setup(String applicationKey, String hmacKey)
    {
        this.applicationKey = applicationKey;
        this.hmacKey = hmacKey;
    }

    private static String BuildRequestBody(Notebook notebook, String language)
    {
        MyScriptStrokeGroup strokeGroup = new MyScriptStrokeGroup();
        foreach (NotebookLine line in notebook.Lines)
        {
            if (line.Type is
                not NotebookLineType.EraseArea and
                not NotebookLineType.Eraser and
                not NotebookLineType.Highlighter1 and
                not NotebookLineType.Highlighter2)
            {
                MyScriptStroke stroke = new MyScriptStroke();

                foreach (NotebookLinePoint point in line.Points)
                {
                    stroke.X.Add(point.X);
                    stroke.Y.Add(point.Y);
                }

                strokeGroup.Strokes.Add(stroke);
            }
        }

        MyScriptBatchInput batchInput = new MyScriptBatchInput(language, strokeGroup);
        return JsonSerializer.Serialize(batchInput, jsonSerializerOptions);
    }

    private String CalculateHmac(String requestBody)
    {
        using HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(this.applicationKey + this.hmacKey));
        Byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(requestBody));
        return BitConverter.ToString(hashBytes).Replace("-", String.Empty, StringComparison.OrdinalIgnoreCase).ToLowerInvariant();
    }
}
