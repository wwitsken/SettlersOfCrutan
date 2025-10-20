using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Domain.PlayerPresence.DomainEvents;

public record PlayerPromotedFromLobbyToGame(
    PlayerId PlayerId, LobbyId FromLobbyId, GameId ToGameId, DateTime OccurredUtc
) : IDomainEvent;
