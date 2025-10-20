using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.PlayerPresence.DomainEvents;

public record PlayerPresenceBecameOnline(PlayerId PlayerId, DateTime OccurredUtc) : IDomainEvent;
