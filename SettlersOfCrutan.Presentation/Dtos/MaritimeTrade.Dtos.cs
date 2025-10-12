using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Presentation.Dtos;

public record MaritimeTradeRequest(ResourceCardType Discard, ResourceCardType Request);
