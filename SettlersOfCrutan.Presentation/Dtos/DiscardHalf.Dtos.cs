namespace SettlersOfCrutan.Presentation.Dtos;

public record DiscardHalfRequest(string PlayerId, List<ResourceCardAmountDto> Discards);
