using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record GameCreatedFromLobbyDomainEvent(GameId GameId, LobbyId SpawnerLobbyId, string[] PlayerUserIds) : IDomainEvent;
