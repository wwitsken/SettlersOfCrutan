using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Domain.PlayerPresence.DomainEvents;

public record PlayerJoinedLobby(PlayerId PlayerId, LobbyId LobbyId, DateTime OccurredUtc) : IDomainEvent;
