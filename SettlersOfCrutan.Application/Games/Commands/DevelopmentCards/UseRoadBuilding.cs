using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Application.Games.Commands.DevelopmentCards;

public record UseRoadBuildingCommand(GameId GameId, PlayerId PlayerId, Edge Edge1, Edge Edge2) : ICommand<(Road r1, Road r2)>;

public sealed class UseRoadBuildingCommandHandler(IGameRepository gameRepository) : ICommandHandler<UseRoadBuildingCommand, (Road r1, Road r2)>
{
    private readonly IGameRepository _gameRepository = gameRepository;

    public async Task<Result<(Road r1, Road r2)>> Handle(UseRoadBuildingCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<(Road, Road)>.Failure(DomainError.NotFound);

        var result = game.PlayRoadBuilding(command.PlayerId, command.Edge1, command.Edge2);
        if (result.IsFailure) return Result<(Road, Road)>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        return saved ? Result<(Road, Road)>.Success(result.Value) : Result<(Road, Road)>.Failure(DomainError.InvalidOperation);
    }
}
