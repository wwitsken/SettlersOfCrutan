using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.PlayerPresence.DomainEvents;

public record PlayerLeftGame(PlayerId PlayerId, GameId GameId, DateTime OccurredUtc) : IDomainEvent;
