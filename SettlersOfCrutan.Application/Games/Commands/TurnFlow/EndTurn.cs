using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.Commands.TurnFlow;

public record EndTurnCommand(GameId GameId) : IGameCommand<PlayerId>;

public sealed class EndTurnCommandHandler(
    IGameRepository gameRepository,
    ICurrentUser currentUser,
    IDateTimeProvider clock) : ICommandHandler<EndTurnCommand, PlayerId>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IDateTimeProvider _clock = clock;

    public async Task<Result<PlayerId>> Handle(EndTurnCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<PlayerId>.Failure(DomainError.NotFound);

        var actor = GamePlayerResolution.ResolveActor(game, await _currentUser.UserId());
        if (actor.IsFailure) return Result<PlayerId>.Failure(actor.Error);

        var result = game.EndTurn(actor.Value, _clock);
        if (result.IsFailure) return Result<PlayerId>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);

        return saved ? Result<PlayerId>.Success(result.Value) : Result<PlayerId>.Failure(new Error("Persistence", "Failed to save game state"));
    }
}
