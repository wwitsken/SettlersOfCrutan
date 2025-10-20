using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.PlayerPresence.DomainEvents;

public record PlayerPresenceTimedOut(PlayerId PlayerId, DateTime LastSeenUtc, DateTime OccurredUtc) : IDomainEvent;
