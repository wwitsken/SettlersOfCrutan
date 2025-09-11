using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record ResourcesDistributedDomainEvent(GameId GameId, Dictionary<PlayerId, List<ResourceCardAmount>> ResourceDistribution) : IDomainEvent;
