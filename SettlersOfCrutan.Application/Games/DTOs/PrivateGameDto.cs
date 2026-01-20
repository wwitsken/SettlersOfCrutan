using SettlersOfCrutan.Application.Games.DomainEventHandlers;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.DTOs;

public record PrivateGameDto
{
    public required PlayerId MyPlayerId { get; set; }
    public required MyHandDto MyHand { get; set; }
    public int MyScore { get; set; }
    public List<List<HexCoordinateDto>> BuildableRoads { get; set; } = [];
    public List<List<HexCoordinateDto>> BuildableSettlements { get; set; } = [];
}

public record MyHandDto(
    IReadOnlyDictionary<ResourceCardType, int> Resources,
    IReadOnlyDictionary<DevelopmentCardType, int> DevCards,
    IReadOnlyDictionary<BuildableType, int> Buildables);