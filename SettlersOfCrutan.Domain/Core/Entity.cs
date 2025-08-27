using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.Core;

public abstract class Entity<TId>()
    : IHasId<TId>
    where TId : BaseId
{
    public abstract TId Id { get; init; }

    public override bool Equals(object? other)
    {
        if (other is null) return false;

        if (GetType() != other.GetType())
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return other is Entity<TId> entity && EqualityComparer<TId>.Default.Equals(Id, entity.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    [JsonIgnore]
    private readonly List<IDomainEvent> _domainEvents = [];

    [JsonIgnore]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent eventItem) => _domainEvents.Add(eventItem);
    protected void RemoveDomainEvent(IDomainEvent eventItem) => _domainEvents.Remove(eventItem);
    protected void ClearDomainEvents() => _domainEvents.Clear();
}
