namespace SettlersOfCrutan.Domain.Games;
public interface IPriceCalculator
{
    List<ResourceAmount> SettlementPrice();
    List<ResourceAmount> CityPrice();
    List<ResourceAmount> RoadPrice();
    List<ResourceAmount> DevelopmentCardPrice();
}
