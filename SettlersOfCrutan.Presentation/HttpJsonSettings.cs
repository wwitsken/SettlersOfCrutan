using SettlersOfCrutan.Infrastructure.Redis.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Presentation;

public static class HttpJsonSettings
{
    public static IServiceCollection AddHttpJsonSettings(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)); // or new(JsonNamingPolicy.CamelCase)
            options.SerializerOptions.Converters.Add(new BaseIdJsonConverterFactory());
        });
        return services;
    }
}
