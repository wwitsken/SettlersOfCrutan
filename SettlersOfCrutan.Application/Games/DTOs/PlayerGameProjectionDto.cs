using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.DTOs;

// Projection of a game for a specific player viewing it
public record PlayerGameProjectionDto
{
    public Guid Id { get; set; }
    public string GameType { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;

    // Board state is public information used for rendering
    public Board Board { get; set; } = null!;

    // Turn / phase context
    public string GamePhase { get; set; } = string.Empty;
    public int Round { get; set; }
    public string CurrentPlayerId { get; set; } = string.Empty;

    // Player list with only public counts for secrecy
    public List<PlayerPublicStateDto> Players { get; set; } = [];

    // The viewing player's own cards (full detail)
    public MyHandDto MyHand { get; set; } = new(
        new Dictionary<ResourceCardType, int>(),
        new Dictionary<DevelopmentCardType, int>(),
        new Dictionary<BuildableType, int>()
    );

    public static PlayerGameProjectionDto FromGame(Game game, string viewingPlayerId) => new()
    {
        Id = game.Id.Value,
        GameType = game.GameType.ToString(),
        GameName = game.GameName,
        Board = game.Board,
        GamePhase = game.GamePhase.ToString(),
        Round = game.Round,
        CurrentPlayerId = game.CurrentPlayerId().Value,
        Players = [.. game.Players.Where(p => p.Id.Value != viewingPlayerId).Select(p => new PlayerPublicStateDto
        {
            PlayerId = p.Id.Value,
            PlayerColor = p.Color,
            ResourceCount = p.TotalResources,
            DevCardCount = p.DevCardCount,
            Buildables = p.GetBuildables()
        })],
        MyHand = new(
            Buildables: game.Players.First(p => p.Id.Value == viewingPlayerId).GetBuildables(),
            Resources: game.Players.First(p => p.Id.Value == viewingPlayerId).GetResources(),
            DevCards: game.Players.First(p => p.Id.Value == viewingPlayerId).GetDevelopmentCards())

    };
}

// Public (non-secret) view of other players' hands
public record PlayerPublicStateDto
{
    public string PlayerId { get; set; } = string.Empty;
    public PlayerColor PlayerColor { get; set; }
    public int ResourceCount { get; set; }
    public int DevCardCount { get; set; }
    public required IReadOnlyDictionary<BuildableType, int> Buildables { get; set; }
}

// Full detail for the viewing player's own hand
public record MyHandDto(
    IReadOnlyDictionary<ResourceCardType, int> Resources,
    IReadOnlyDictionary<DevelopmentCardType, int> DevCards,
    IReadOnlyDictionary<BuildableType, int> Buildables
);
