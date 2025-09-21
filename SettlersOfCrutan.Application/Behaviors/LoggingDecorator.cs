using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Application.Behaviors;
internal static class LoggingDecorator
{
    internal sealed class QueryHandler<TQuery, TResponse>(ILogger<QueryHandler<TQuery, TResponse>> logger)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public Task<Result<TResponse>> Handle(TQuery query, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
