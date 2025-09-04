using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record ResourcesDistributedDomainEvent(GameId GameId, Dictionary<PlayerId, List<ResourceAmount>> ResourceDistribution) : IDomainEvent;
