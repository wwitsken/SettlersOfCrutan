using System.Text.Json;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Infrastructure.Redis.Serialization;
public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = Create();

    private static JsonSerializerOptions Create()
    {
        var opts = new JsonSerializerOptions
        {
            //IncludeFields = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.Strict
        };

        opts.Converters.Add(new JsonStringEnumConverter());
        opts.Converters.Add(new BaseIdJsonConverterFactory());

        return opts;
    }
}