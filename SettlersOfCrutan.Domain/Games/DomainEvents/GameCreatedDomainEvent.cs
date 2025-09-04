using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record GameCreatedDomainEvent(GameId gameId, string[] playerUserIds) : IDomainEvent;
