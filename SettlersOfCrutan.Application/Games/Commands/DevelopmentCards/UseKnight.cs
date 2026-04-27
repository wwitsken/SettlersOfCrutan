using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
namespace SettlersOfCrutan.Application.Games.Commands.DevelopmentCards;

public record UseKnightCommand(GameId GameId) : IGameCommand;

public sealed class UseKnightCommandHandler(
    IGameRepository gameRepository,
    ICurrentUser currentUser) : ICommandHandler<UseKnightCommand>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<Nothing>> Handle(UseKnightCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result.Failure(DomainError.NotFound);

        var actor = GamePlayerResolution.ResolveActor(game, await _currentUser.UserId());
        if (actor.IsFailure) return Result.Failure(actor.Error);

        var result = game.PlayKnight(actor.Value);
        if (result.IsFailure) return Result.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        if (!saved) return Result.Failure(new Error("Persistence", "Failed to save game state"));

        return Result.Success();
    }
}
