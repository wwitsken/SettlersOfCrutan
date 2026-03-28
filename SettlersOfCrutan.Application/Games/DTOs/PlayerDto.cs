using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.DTOs;

public record PlayerDto
{
    public required string Id { get; set; }
    public required int PlayOrder { get; set; }
    public bool IsPlaying { get; set; } = false;
    public string DisplayName { get; set; } = "";
    public PlayerColor PlayerColor { get; set; }
    public int ResourceCardCount { get; set; }
    public int DevelopmentCardCount { get; set; }
    public Dictionary<BuildableType, int> PieceReserve { get; set; } = [];
    public int DiscardRequirement { get; set; } = 0;
    /// <summary>Visible VP from settlements/cities only (not hidden VP dev cards).</summary>
    public int VictoryPoints { get; set; }
    public bool HasLongestRoad { get; set; }
    public bool HasLargestArmy { get; set; }
}