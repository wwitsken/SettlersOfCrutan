using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record MonopolyCardPlayedDomainEvent(
    GameId GameId,
    PlayerId PlayerId,
    ResourceCardType ResourceType,
    Dictionary<PlayerId, int> StolenPerPlayer,
    int TotalStolen
) : IDomainEvent;
