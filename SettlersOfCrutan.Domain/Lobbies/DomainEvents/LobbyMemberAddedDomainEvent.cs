using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Lobbies.DomainEvents;
public record LobbyMemberAddedDomainEvent(LobbyId LobbyId, LobbyMember LobbyMember) : IDomainEvent;