using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Application.Games.Commands.Build;

public record BuildInitialCommand(GameId GameId, PlayerId PlayerId, Vertex SettlementCoordinate, Edge RoadCoordinate) : ICommand;
public sealed class BuildInitialCommandHandler(IGameRepository gameRepository, IDateTimeProvider clock) : ICommandHandler<BuildInitialCommand>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IDateTimeProvider _clock = clock;

    public async Task<Result<Nothing>> Handle(BuildInitialCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);

        if (game is null) return Result<Nothing>.Failure(DomainError.NotFound);

        var result = game.PlaceInitial(command.PlayerId, command.SettlementCoordinate, command.RoadCoordinate, _clock);

        if (result.IsFailure) return Result<Nothing>.Failure(result.Error);

        await _gameRepository.SaveAsync(game, ct);

        return Result<Nothing>.Success();
    }
}
