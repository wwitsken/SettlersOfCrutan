using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games;
public interface IPriceCalculator
{
    List<ResourceCardAmount> SettlementPrice();
    List<ResourceCardAmount> CityPrice();
    List<ResourceCardAmount> RoadPrice();
    List<ResourceCardAmount> DevelopmentCardPrice();
}
