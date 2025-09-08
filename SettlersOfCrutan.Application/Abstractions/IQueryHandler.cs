using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Application.Abstractions;

public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> Handle(TQuery query, CancellationToken ct = default);
}
