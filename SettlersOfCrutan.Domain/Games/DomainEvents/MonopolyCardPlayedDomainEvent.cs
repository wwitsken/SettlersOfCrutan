using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record MonopolyCardPlayedDomainEvent(
    GameId GameId,
    PlayerId PlayerId,
    ResourceType ResourceType,
    Dictionary<PlayerId, int> StolenPerPlayer,
    int TotalStolen
) : IDomainEvent;
