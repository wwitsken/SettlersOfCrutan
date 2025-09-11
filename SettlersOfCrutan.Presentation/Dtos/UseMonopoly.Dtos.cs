using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Presentation.Dtos;

public record UseMonopolyRequest(string PlayerId, ResourceCardType ResourceType);
