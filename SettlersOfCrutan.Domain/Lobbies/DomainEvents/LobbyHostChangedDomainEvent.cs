using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Lobbies.DomainEvents;
public record LobbyHostChangedDomainEvent(LobbyId LobbyId, string UserId) : IDomainEvent;
