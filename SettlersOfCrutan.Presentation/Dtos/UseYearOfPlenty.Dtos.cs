using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Presentation.Dtos;

public record UseYearOfPlentyRequest(string PlayerId, ResourceCardType Resource1, ResourceCardType Resource2);
