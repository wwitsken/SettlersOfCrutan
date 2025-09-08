using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.Commands.Lifecycle;

public record EndGameCommand(GameId GameId) : ICommand<GameId>;
public sealed class EndGame : ICommandHandler<EndGameCommand, GameId>
{
    public Task<Result<GameId>> Handle(EndGameCommand command, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
