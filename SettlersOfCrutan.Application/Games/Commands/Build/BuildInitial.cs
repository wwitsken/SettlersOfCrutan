using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Application.Games.Commands.Build;

public record BuildInitialCommand(GameId GameId, Vertex SettlementCoordinate, Edge RoadCoordinate) : IGameCommand;
public sealed class BuildInitialCommandHandler(IGameRepository gameRepository,
                                                ICurrentUser currentUser,
                                               IDateTimeProvider clock) : ICommandHandler<BuildInitialCommand>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IDateTimeProvider _clock = clock;

    public async Task<Result<Nothing>> Handle(BuildInitialCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<Nothing>.Failure(DomainError.NotFound);
        var actor = GamePlayerResolution.ResolveActor(game, await _currentUser.UserId());
        if (actor.IsFailure) return Result<Nothing>.Failure(actor.Error);
        var result = game.PlaceInitial(actor.Value, command.SettlementCoordinate, command.RoadCoordinate, _clock);
        if (result.IsFailure) return Result<Nothing>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        if (!saved) return Result<Nothing>.Failure(new Error("Persistence", "Failed to save game state"));

        return Result<Nothing>.Success();
    }
}