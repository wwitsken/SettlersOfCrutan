using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.Commands.TurnFlow;

public record RollDiceCommandResult(int Dice1, int Dice2);
public record RollDiceCommand(GameId GameId) : IGameCommand<RollDiceCommandResult>;
public sealed class RollDiceCommandHandler(
    IGameRepository gameRepository,
    ICurrentUser currentUser) : ICommandHandler<RollDiceCommand, RollDiceCommandResult>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<RollDiceCommandResult>> Handle(RollDiceCommand command, CancellationToken ct = default)
    {
        Game? game = await _gameRepository.GetAsync(command.GameId, ct);

        if (game is null) return Result<RollDiceCommandResult>.Failure(new Error("NotFound", "Game not found"));

        var actor = GamePlayerResolution.ResolveActor(game, await _currentUser.UserId());
        if (actor.IsFailure) return Result<RollDiceCommandResult>.Failure(actor.Error);

        var result = game.RollAndResolveProduction(actor.Value);

        if (result.IsFailure) return Result.Failure<RollDiceCommandResult>(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);

        return saved
            ? Result.Success(new RollDiceCommandResult(result.Value.Item1, result.Value.Item2))
            : Result<RollDiceCommandResult>.Failure(new Error("Persistence", "Failed to save game state"));
    }
}
