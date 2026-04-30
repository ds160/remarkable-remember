using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using ReMarkableRemember.Common.Notebook;

namespace ReMarkableRemember.Services.HandWritingRecognitionService;

public partial class HandWritingRecognitionServiceMyScript
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static String BuildJsonRequest(Page page, String language)
    {
        List<JsonRequest.Stroke> strokes = new List<JsonRequest.Stroke>();
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
                strokes.Add(new JsonRequest.Stroke(x, y));
            }
        }

        JsonRequest jsonRequest = new JsonRequest(language, page.Resolution, strokes);
        return JsonSerializer.Serialize(jsonRequest, jsonSerializerOptions);
    }

    private sealed class JsonRequest
    {
        public JsonRequest(String language, Int32 resolution, List<Stroke> strokes)
        {
            this.Configuration = new { Lang = language };
            this.ContentType = "Text";
            this.XDPI = resolution;
            this.YDPI = resolution;

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
