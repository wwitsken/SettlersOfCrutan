using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.BuildSettlement;

public class BoardMustAllowSettlementPlacement : ISpecification<BuildSettlementContext>
{
    public Result<Nothing> IsSatisfiedBy(BuildSettlementContext context) =>
        context.Board.CanPlaceSettlement(context.ActingPlayerId, context.Vertex);
}
