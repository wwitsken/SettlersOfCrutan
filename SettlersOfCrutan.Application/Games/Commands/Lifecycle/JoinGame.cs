using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.Commands.Lifecycle;
public record JoinGameCommand(GameId GameId) : IGameCommand<GameId>;
public sealed class JoinGameCommandHandler(IGameRepository gameRepository,
                                           ICurrentUser currentUser,
                                           IDateTimeProvider clock) : ICommandHandler<JoinGameCommand, GameId>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IDateTimeProvider _clock = clock;

    public async Task<Result<GameId>> Handle(JoinGameCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);

        if (game is null)
            return Result<GameId>.Failure(new Error("NotFound", "Game not found"));

        var joinResult = game.JoinPlayer(await _currentUser.UserId(), _clock.UtcNow);

        if (joinResult.IsFailure)
            return Result<GameId>.Failure(joinResult.Error);

        if (game.AllPlayersJoined())
            game.StartGame(_clock /*, TimeDuration */);

        var saved = await _gameRepository.SaveAsync(game, ct);

        return saved ? Result<GameId>.Success(game.Id) : Result<GameId>.Failure(new Error("Persistence", "Failed to save game state"));
    }
}
