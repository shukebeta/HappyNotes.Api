using System.Text.Json;
using System.Text.Json.Serialization;

namespace HappyNotes.Common;

public static class JsonSerializerConfig
{
    /// <summary>
    /// Shared JsonSerializerOptions for consistent serialization across the application.
    /// Use this instead of creating new JsonSerializerOptions instances or using defaults.
    /// </summary>
    public static JsonSerializerOptions Default { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Converters = 
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    /// <summary>
    /// JsonSerializerOptions for pretty-printed JSON (debugging/logging purposes).
    /// </summary>
    public static JsonSerializerOptions Indented { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Converters = 
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };
}