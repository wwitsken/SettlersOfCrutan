using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Infrastructure.Redis.Serialization;
using System.Text.Json;

namespace SettlersOfCrutan.Infrastructure.Outbox;

public record OutboxEnvelope
(
    string Aggregate,
    string AggregateId,
    long AggregateVersion,
    string EventType,
    string EventId,
    DateTimeOffset OccurredAt,
    string Payload,
    int Attempts = 0
)
{
    public static OutboxEnvelope Create(string agg, string aggId, long version, IDomainEvent ev)
    {
        var payload = JsonSerializer.Serialize(ev, ev.GetType(), JsonOptions.Default);
        return new OutboxEnvelope(
            Aggregate: agg,
            AggregateId: aggId,
            AggregateVersion: version,
            EventType: ev.GetType().FullName ?? ev.GetType().Name,
            EventId: Guid.NewGuid().ToString("N"),
            OccurredAt: DateTimeOffset.UtcNow,
            Payload: payload
        );
    }
}
