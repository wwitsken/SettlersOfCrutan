using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.BuildCity;

public class BoardMustAllowCityUpgrade : ISpecification<BuildCityContext>
{
    public Result<Nothing> IsSatisfiedBy(BuildCityContext context) =>
        context.Board.CanUpgradeToCity(context.ActingPlayerId, context.Vertex);
}
