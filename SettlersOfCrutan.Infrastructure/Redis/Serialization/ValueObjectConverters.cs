using System.Text.Json;
using System.Text.Json.Serialization;
using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Infrastructure.Redis.Serialization;

// Generic converter for BaseId<TValue>
public sealed class BaseIdJsonConverter<TId, TValue> : JsonConverter<TId>
    where TId : BaseId<TValue>, new()
{
    public override TId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Deserialize the underlying value directly (Guid, string, complex object, etc.)
        var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
        return new TId { Value = value! };
    }

    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}