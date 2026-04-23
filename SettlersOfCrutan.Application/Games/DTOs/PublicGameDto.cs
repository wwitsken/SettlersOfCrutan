using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.DTOs;

public sealed record PublicGameDto
{
    public Guid Id { get; set; }
    public required string GameType { get; set; }
    public required string GameName { get; set; }
    public required BoardDto Board { get; set; }
    public Dictionary<ResourceCardType, int> BankResourceHand { get; set; } = [];
    public Dictionary<DevelopmentCardType, int> BankDevCardHand { get; set; } = [];
    public DateTimeOffset? TurnExpiresAt { get; set; }
    public PlayerDirection PlayerDirection { get; set; }
    public GamePhase GamePhase { get; set; }
    public int Round { get; set; }
    public PlayerId? CurrentPlayerId { get; set; }
    public TradeOfferDto? CurrentTradeOffer { get; set; }
    public List<PlayerDto> Players { get; set; } = [];
    /// <summary>
    /// Set once <see cref="GamePhase"/> is <see cref="GamePhase.GameEnd"/>. Lets late joiners /
    /// reload clients resolve the winner from a plain GET without replaying SignalR events.
    /// </summary>
    public string? WinnerPlayerId { get; set; }
}
