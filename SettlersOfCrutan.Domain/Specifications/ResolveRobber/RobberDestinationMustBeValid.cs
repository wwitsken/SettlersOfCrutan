using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.ResolveRobber;

public class RobberDestinationMustBeValid : ISpecification<ResolveRobberContext>
{
    public Result<Nothing> IsSatisfiedBy(ResolveRobberContext context) =>
        context.Board.CanMoveRobberTo(context.NewRobberHexCoord)
            ? Result.Success()
            : Result.Failure(DomainError.InvalidRobberMove);
}
