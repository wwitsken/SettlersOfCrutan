using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Application.Games.Commands.Lifecycle;
public record JoinGameCommand(Guid GameId, string PlayerId) : ICommand<Guid>;
public sealed class JoinGameCommandHandler(IGameRepository gameRepository, IDateTimeProvider clock) : ICommandHandler<JoinGameCommand, Guid>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IDateTimeProvider _clock = clock;

    public async Task<Result<Guid>> Handle(JoinGameCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(new() { Value = command.GameId }, ct);

        if (game is null)
            return Result<Guid>.Failure(new Error("NotFound", "Game not found"));

        var joinResult = game.JoinPlayer(new() { Value = command.PlayerId }, DateTimeOffset.Now);

        if (joinResult.IsFailure)
            return Result<Guid>.Failure(joinResult.Error);

        if (game.AllPlayersJoined())
        {
            game.StartGame(_clock /*, TimeDuration */);
        }

        var saved = await _gameRepository.SaveAsync(game, ct);

        return saved ? Result<Guid>.Success(game.Id.Value) : Result<Guid>.Failure(new Error("Persistence", "Failed to save game state"));
    }
}
