using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record RobPlayerStartedDomainEvent(GameId GameId) : IDomainEvent;
