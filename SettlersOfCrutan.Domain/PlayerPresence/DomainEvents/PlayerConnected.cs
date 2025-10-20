using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.PlayerPresence.DomainEvents;

public record PlayerConnected(PlayerId PlayerId, ConnectionId ConnectionId, DateTime OccurredUtc) : IDomainEvent;
