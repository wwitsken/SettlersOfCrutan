using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.AcceptTrade;

public class BothPlayersMustExist : ISpecification<AcceptTradeContext>
{
    public Result<Nothing> IsSatisfiedBy(AcceptTradeContext context) =>
        context.Proposer is not null && context.Acceptor is not null
            ? Result.Success()
            : Result.Failure(DomainError.NotFound);
}
