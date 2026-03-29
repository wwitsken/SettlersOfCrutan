using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.ProposeTrade;

public class ProposerMustHaveResources : ISpecification<ProposeTradeContext>
{
    public Result<Nothing> IsSatisfiedBy(ProposeTradeContext context) =>
        context.Proposer is not null && context.Proposer.HasAtLeast(context.Offered)
            ? Result.Success()
            : Result.Failure(DomainError.InsufficientResources);
}
