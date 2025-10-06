using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Lobbies.DomainEvents;
public record LobbyMemberRemovedDomainEvent(LobbyId LobbyId, LobbyMember LobbyMember) : IDomainEvent;
