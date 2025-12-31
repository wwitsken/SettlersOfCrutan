using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.DTOs;

public record TradeOfferDto
{
    public string ProposerId { get; set; }
    public string? AcceptorId { get; set; }
    public Dictionary<ResourceCardType, int> RequestedResources { get; set; } = [];
    public Dictionary<ResourceCardType, int> OfferedResources { get; set; } = [];
    public bool IsAccepted { get; set; }
}
