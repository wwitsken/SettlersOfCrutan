using System.Linq;
using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.ResolveRobber;

public class RobberVictimMustBeValid : ISpecification<ResolveRobberContext>
{
    public Result<Nothing> IsSatisfiedBy(ResolveRobberContext context)
    {
        if (context.StealTargets.Count == 0)
        {
            return context.VictimId is not null
                ? Result.Failure(DomainError.RobberVictimNotAllowed)
                : Result.Success();
        }

        if (context.VictimId is null)
            return Result.Failure(DomainError.RobberVictimRequired);

        if (!context.StealTargets.Any(t => t.Equals(context.VictimId)))
            return Result.Failure(DomainError.RobberVictimNotEligible);

        return Result.Success();
    }
}
