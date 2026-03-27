using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.Core;
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : BaseId
{
    // Version for optimistic concurrency (starts at 0)
    [JsonInclude]
    public long Version { get; protected set; }

    public void BumpVersion() => Version++;

    /// <summary>Clears raised domain events after a successful persistence publish so they are not replayed.</summary>
    public void ClearRaisedDomainEvents() => ClearDomainEvents();
}
