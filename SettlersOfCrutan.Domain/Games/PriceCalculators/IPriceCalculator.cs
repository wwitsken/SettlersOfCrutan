namespace SettlersOfCrutan.Domain.Games.PriceCalculators;
public interface IPriceCalculator
{
    List<ResourceAmount> SettlementPrice();
    List<ResourceAmount> CityPrice();
    List<ResourceAmount> RoadPrice();
    List<ResourceAmount> DevelopmentCardPrice();
}
