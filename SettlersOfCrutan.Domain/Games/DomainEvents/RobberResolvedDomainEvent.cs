using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record RobberResolvedDomainEvent(GameId Id, HexCoord newRobberHexCoord, ResourceCardType StolenResourceType, PlayerId RobberId, PlayerId VictimId) : IDomainEvent;
