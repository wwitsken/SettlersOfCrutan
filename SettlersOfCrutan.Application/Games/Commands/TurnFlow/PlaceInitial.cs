using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Application.Games.Commands.TurnFlow;

public record PlaceInitialCommand(Guid GameId, string PlayerId, Vertex SettlementCoordinate, Edge RoadCoordinate) : ICommand;
public sealed class PlaceInitialCommandHandler(IGameRepository gameRepository) : ICommandHandler<PlaceInitialCommand>
{
    private readonly IGameRepository _gameRepository = gameRepository;

    public async Task<Result<Nothing>> Handle(PlaceInitialCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(command.PlayerId);

        var game = await _gameRepository.GetAsync(new GameId() { Value = command.GameId }, ct);

        if (game is null) return Result<Nothing>.Failure(DomainError.NotFound);

        var result = game.PlaceInitial(new() { Value = command.PlayerId }, command.SettlementCoordinate, command.RoadCoordinate);

        if (result.IsFailure) return Result<Nothing>.Failure(result.Error);

        await _gameRepository.SaveAsync(game, ct);

        return Result<Nothing>.Success();
    }
}
