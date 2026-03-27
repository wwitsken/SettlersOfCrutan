using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Lobbies.DomainEvents;
public record LobbyMemberRemovedDomainEvent(LobbyId LobbyId, LobbyMemberId LobbyMemberId) : IDomainEvent;
