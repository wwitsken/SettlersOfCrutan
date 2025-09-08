using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record YearOfPlentyCardPlayedDomainEvent(GameId GameId, PlayerId PlayerId, ResourceType ResourceType1, ResourceType ResourceType2) : IDomainEvent;
