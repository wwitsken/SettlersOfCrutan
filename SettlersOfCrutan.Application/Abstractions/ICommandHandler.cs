using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Application.Abstractions;
public interface ICommandHandler<T>
    where T : ICommand
{
    Task<Result<Nothing>> Handle(T command, CancellationToken ct = default);
}

public interface ICommandHandler<T, TResult>
    where T : ICommand<TResult>
{
    Task<Result<TResult>> Handle(T command, CancellationToken ct = default);
}