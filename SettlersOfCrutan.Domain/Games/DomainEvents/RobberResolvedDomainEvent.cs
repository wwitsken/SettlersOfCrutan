using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record RobberResolvedDomainEvent(GameId Id, HexCoord newRobberHexCoord, ResourceType StolenResourceType, PlayerId RobberId, PlayerId VictimId) : IDomainEvent;
