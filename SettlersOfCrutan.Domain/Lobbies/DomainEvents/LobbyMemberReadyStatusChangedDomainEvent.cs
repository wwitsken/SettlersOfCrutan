using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Lobbies.DomainEvents;
public record LobbyMemberReadyStatusChangedDomainEvent(LobbyId LobbyId, LobbyMemberId LobbyMemberId, bool IsReady) : IDomainEvent;
