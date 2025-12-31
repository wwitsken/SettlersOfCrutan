using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.DTOs;

public record PrivatePlayerDto
{
    public Dictionary<ResourceCardType, int> ResourceHand { get; set; } = [];
    public Dictionary<DevelopmentCardType, int> DevCardHand { get; set; } = [];
}

public record PublicPlayerDto
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
}