using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Lobbies.DomainEvents;
public record LobbyMemberReadyStatusChangedDomainEvent(LobbyId LobbyId, string UserId, bool IsReady) : IDomainEvent;
