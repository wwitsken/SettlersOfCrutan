using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.DTOs;

public record PrivateGameDto
{
    public required PlayerId MyPlayerId { get; set; }
    public required MyHandDto MyHand { get; set; }
    public int MyScore { get; set; }
    /// <summary>Face-up dev plays for the current user (knight count, monopoly, etc.).</summary>
    public Dictionary<DevelopmentCardType, int> PlayedDevelopmentCards { get; set; } = [];
    public List<List<HexCoordinateDto>> BuildableRoads { get; set; } = [];
    public List<List<HexCoordinateDto>> BuildableSettlements { get; set; } = [];
}

public record MyHandDto(
    IReadOnlyDictionary<ResourceCardType, int> Resources,
    IReadOnlyDictionary<DevelopmentCardType, int> DevCards,
    IReadOnlyDictionary<BuildableType, int> Buildables);