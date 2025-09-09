using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.Commands.TurnFlow;

public record RollDiceCommand(GameId GameId, PlayerId PlayerId) : ICommand<(int d1, int d2)>;
public sealed class RollDiceCommandHandler(IGameRepository gameRepository) : ICommandHandler<RollDiceCommand, (int, int)>
{
    private readonly IGameRepository _gameRepository = gameRepository;

    public async Task<Result<(int, int)>> Handle(RollDiceCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);

        if (game is null) return Result<(int, int)>.Failure(new Error("NotFound", "Game not found"));

        var result = game.RollAndResolveProduction(command.PlayerId);

        if (result.IsFailure) return result;

        var saved = await _gameRepository.SaveAsync(game, ct);

        return saved ? result : Result<(int, int)>.Failure(new Error("Persistence", "Failed to save game state"));
    }
}
