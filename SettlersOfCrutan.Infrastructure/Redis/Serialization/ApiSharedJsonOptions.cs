using System.Text.Json;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Infrastructure.Redis.Serialization;

/// <summary>
/// HTTP API and SignalR hub payloads use the same JSON shape (camelCase, string enums, BaseId flattening).
/// </summary>
public static class ApiSharedJsonOptions
{
    public static void ApplyTo(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.NumberHandling = JsonNumberHandling.Strict;

        if (!HasConverter<JsonStringEnumConverter>(options))
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        if (!HasConverter<BaseIdJsonConverterFactory>(options))
            options.Converters.Add(new BaseIdJsonConverterFactory());
    }

    private static bool HasConverter<T>(JsonSerializerOptions options) =>
        options.Converters.Any(c => c is T);

    /// <summary>Fresh options for SignalR <see cref="Microsoft.AspNetCore.SignalR.JsonHubProtocolOptions"/>.</summary>
    public static JsonSerializerOptions CreateForSignalR()
    {
        var o = new JsonSerializerOptions();
        ApplyTo(o);
        return o;
    }
}
