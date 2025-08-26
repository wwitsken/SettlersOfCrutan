using System.Text.Json;
using System.Text.Json.Serialization;
using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Infrastructure.Redis.Serialization;
public sealed class BaseIdJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeof(BaseId).IsAssignableFrom(typeToConvert);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(BaseIdJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}