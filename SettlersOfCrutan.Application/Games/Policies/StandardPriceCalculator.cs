using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Policies;
public class StandardPriceCalculator : IPriceCalculator
{
    public List<ResourceCardAmount> CityPrice() => [
        new ResourceCardAmount(ResourceCardType.Ore, 3),
        new ResourceCardAmount(ResourceCardType.Grain, 2)
        ];
    public List<ResourceCardAmount> DevelopmentCardPrice() => [
        new ResourceCardAmount(ResourceCardType.Ore, 1),
        new ResourceCardAmount(ResourceCardType.Wool, 1),
        new ResourceCardAmount(ResourceCardType.Grain, 1)
        ];
    public List<ResourceCardAmount> RoadPrice() => [
        new ResourceCardAmount(ResourceCardType.Brick, 1),
        new ResourceCardAmount(ResourceCardType.Lumber, 1)
        ];
    public List<ResourceCardAmount> SettlementPrice() => [
        new ResourceCardAmount(ResourceCardType.Brick, 1),
        new ResourceCardAmount(ResourceCardType.Lumber, 1),
        new ResourceCardAmount(ResourceCardType.Wool, 1),
        new ResourceCardAmount(ResourceCardType.Grain, 1)
        ];
}
