using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Abstractions;
public interface ICommand { }
public interface ICommand<TResult> { }

public interface IGameCommand : ICommand
{
    GameId GameId { get; }
}
public interface IGameCommand<TResult> : ICommand<TResult>, IGameCommand;
