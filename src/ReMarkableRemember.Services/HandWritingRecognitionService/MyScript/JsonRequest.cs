using System;
using System.Collections.Generic;
using System.Text.Json;
using ReMarkableRemember.Common.Notebook;
using ReMarkableRemember.Common.Notebook.Enumerations;

namespace ReMarkableRemember.Services.HandWritingRecognitionService.MyScript;

internal static class JsonRequest
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static String Build(Page page, String language)
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
}
