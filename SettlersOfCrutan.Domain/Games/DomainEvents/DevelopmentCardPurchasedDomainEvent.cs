using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record DevelopmentCardPurchasedDomainEvent(GameId GameId, PlayerId PlayerId, DevelopmentCardType DevelopmentCardType) : IDomainEvent;
