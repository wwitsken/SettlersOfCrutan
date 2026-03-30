using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.DTOs;

public record TradeOfferDto
{
    public Guid Id { get; set; }
    public string PlayerProposerId { get; set; }
    public string? PlayerAcceptorId { get; set; }
    public Dictionary<ResourceCardType, int> RequestedResources { get; set; } = [];
    public Dictionary<ResourceCardType, int> OfferedResources { get; set; } = [];
    public bool IsAccepted { get; set; }
}
