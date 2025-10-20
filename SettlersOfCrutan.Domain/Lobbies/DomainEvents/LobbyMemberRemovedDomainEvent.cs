using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Lobbies.DomainEvents;
public record LobbyMemberRemovedDomainEvent(LobbyId LobbyId, PlayerId PlayerId) : IDomainEvent;
