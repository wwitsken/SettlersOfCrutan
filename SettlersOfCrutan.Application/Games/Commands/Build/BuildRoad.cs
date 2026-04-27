using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Games;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Application.Games.Policies;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Application.Games.Commands.Build;

public record BuildRoadCommand(GameId GameId, Edge Edge) : IGameCommand;

public sealed class BuildRoadCommandHandler(IGameRepository gameRepository,
                                               ICurrentUser currentUser,
                                               StandardPriceCalculator priceCalculator) : ICommandHandler<BuildRoadCommand>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly StandardPriceCalculator _priceCalculator = priceCalculator;

    public async Task<Result<Nothing>> Handle(BuildRoadCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<Nothing>.Failure(DomainError.NotFound);

        var actor = GamePlayerResolution.ResolveActor(game, await _currentUser.UserId());
        if (actor.IsFailure) return Result<Nothing>.Failure(actor.Error);
        var result = game.BuildRoad(_priceCalculator, actor.Value, command.Edge);
        if (result.IsFailure) return Result<Nothing>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        if (!saved) return Result<Nothing>.Failure(new Error("Persistence", "Failed to save game state"));

        return Result<Nothing>.Success();
    }
}
