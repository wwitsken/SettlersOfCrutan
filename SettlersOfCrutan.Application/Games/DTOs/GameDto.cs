using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.DTOs;

public sealed record GameDto
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
    public int PlayerIndex { get; set; }
    public TradeOfferDto? CurrentTradeOffer { get; set; }
    public List<PublicPlayerDto> Players { get; set; } = [];
}
