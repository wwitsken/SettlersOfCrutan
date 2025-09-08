namespace SettlersOfCrutan.Domain.Games.PriceCalculators;
public class StandardPriceCalculator : IPriceCalculator
{
    public List<ResourceAmount> CityPrice() => [
        new ResourceAmount(ResourceType.Ore, 3),
        new ResourceAmount(ResourceType.Grain, 2)
        ];
    public List<ResourceAmount> DevelopmentCardPrice() => [
        new ResourceAmount(ResourceType.Ore, 1),
        new ResourceAmount(ResourceType.Wool, 1),
        new ResourceAmount(ResourceType.Grain, 1)
        ];
    public List<ResourceAmount> RoadPrice() => [
        new ResourceAmount(ResourceType.Brick, 1),
        new ResourceAmount(ResourceType.Lumber, 1)
        ];
    public List<ResourceAmount> SettlementPrice() => [
        new ResourceAmount(ResourceType.Brick, 1),
        new ResourceAmount(ResourceType.Lumber, 1),
        new ResourceAmount(ResourceType.Wool, 1),
        new ResourceAmount(ResourceType.Grain, 1)
        ];
}
