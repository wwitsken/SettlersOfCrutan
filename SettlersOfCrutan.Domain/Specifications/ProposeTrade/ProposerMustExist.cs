using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.ProposeTrade;

public class ProposerMustExist : ISpecification<ProposeTradeContext>
{
    public Result<Nothing> IsSatisfiedBy(ProposeTradeContext context) =>
        context.Proposer is not null
            ? Result.Success()
            : Result.Failure(DomainError.NotFound);
}
