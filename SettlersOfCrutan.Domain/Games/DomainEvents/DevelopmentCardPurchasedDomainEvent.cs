using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record DevelopmentCardPurchasedDomainEvent(GameId GameId, PlayerId PlayerId, DevelopmentCardType DevelopmentCardType) : IDomainEvent;
