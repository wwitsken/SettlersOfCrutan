using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games;
public record GameCreatedDomainEvent(GameId gameId, string[] playerUserIds) : IDomainEvent;
