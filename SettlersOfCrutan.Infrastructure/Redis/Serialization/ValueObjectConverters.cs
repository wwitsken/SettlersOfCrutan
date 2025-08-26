using System.Text.Json;
using System.Text.Json.Serialization;
using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Infrastructure.Redis.Serialization;

public sealed class BaseIdJsonConverter<TId> : JsonConverter<TId>
    where TId : BaseId, new()
{
    public override TId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Accept both string and Guid tokens
        Guid guid = reader.TokenType switch
        {
            JsonTokenType.String => reader.TryGetGuid(out var g) ? g : Guid.Parse(reader.GetString()!),
            JsonTokenType.Number => Guid.Parse(reader.GetString()!), // unlikely, but defensive
            _ => throw new JsonException($"Expected string GUID for {typeof(TId).Name}")
        };

        var id = new TId { Value = guid };
        return id;
    }

    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options)
    {
        if (value is null) { writer.WriteNullValue(); return; }
        writer.WriteStringValue(value.Value);
    }
}