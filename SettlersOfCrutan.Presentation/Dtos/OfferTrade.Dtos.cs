using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Presentation.Dtos;

public record OfferTradeRequest(string PlayerId, List<ResourceCardAmountDto> Requested, List<ResourceCardAmountDto> Offered);
public record ResourceCardAmountDto(ResourceCardType Type, int Quantity);
public static class ResourceCardAmountDtoExtensions
{
    public static ResourceCardAmount ToDomain(this ResourceCardAmountDto dto) => new(dto.Type, dto.Quantity);
}
