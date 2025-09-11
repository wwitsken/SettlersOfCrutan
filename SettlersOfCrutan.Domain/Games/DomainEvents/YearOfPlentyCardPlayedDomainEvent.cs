using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record YearOfPlentyCardPlayedDomainEvent(GameId GameId, PlayerId PlayerId, ResourceCardType ResourceType1, ResourceCardType ResourceType2) : IDomainEvent;
