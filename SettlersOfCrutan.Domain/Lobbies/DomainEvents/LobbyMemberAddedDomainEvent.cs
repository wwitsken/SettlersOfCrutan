using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Lobbies.DomainEvents;
public record LobbyMemberAddedDomainEvent(LobbyId LobbyId, LobbyMemberId LobbyMemberId) : IDomainEvent;