using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Presentation.Dtos;

public record MaritimeTradeRequest(string PlayerId, ResourceCardType Discard, ResourceCardType Request);
