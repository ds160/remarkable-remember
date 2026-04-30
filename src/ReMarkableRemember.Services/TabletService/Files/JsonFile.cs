using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReMarkableRemember.Services.TabletService.Files;

internal static class JsonFile
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static T Deserialize<T>(String fileText) where T : struct
    {
        return JsonSerializer.Deserialize<T>(fileText, jsonSerializerOptions);
    }

    public static String Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, jsonSerializerOptions);
    }
}
