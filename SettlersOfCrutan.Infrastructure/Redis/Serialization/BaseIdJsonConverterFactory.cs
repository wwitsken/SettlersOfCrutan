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
        // Walk up the hierarchy to find BaseId<TValue>
        var baseType = typeToConvert;
        while (baseType is not null && baseType != typeof(object))
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(BaseId<>))
            {
                var valueType = baseType.GetGenericArguments()[0];
                var converterType = typeof(BaseIdJsonConverter<,>).MakeGenericType(typeToConvert, valueType);
                return (JsonConverter)Activator.CreateInstance(converterType)!;
            }
            baseType = baseType.BaseType!;
        }

        throw new InvalidOperationException($"Type {typeToConvert} does not inherit from BaseId<T>.");
    }
}