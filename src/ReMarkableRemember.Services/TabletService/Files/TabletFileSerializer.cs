using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ReMarkableRemember.Services.TabletService.Files.Interfaces;

namespace ReMarkableRemember.Services.TabletService.Files;

internal sealed class TabletFileSerializer : ITabletFileSerializer
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public T Deserialize<T>(String fileText) where T : struct, ITabletFile
    {
        return JsonSerializer.Deserialize<T>(fileText, jsonSerializerOptions);
    }

    public String Serialize<T>(T value) where T : struct, ITabletFile
    {
        return JsonSerializer.Serialize(value, jsonSerializerOptions);
    }
}
