using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.TurnFlow;

public record ResolveRobberCommand(GameId GameId, HexCoord NewRobberHexCoord, PlayerId? VictimId) : IGameCommand<ResourceCardType>;

public sealed class ResolveRobberCommandHandler(
    IGameRepository gameRepository,
    ICurrentUser currentUser) : ICommandHandler<ResolveRobberCommand, ResourceCardType>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<ResourceCardType>> Handle(ResolveRobberCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<ResourceCardType>.Failure(DomainError.NotFound);

        var actor = GamePlayerResolution.ResolveActor(game, await _currentUser.UserId());
        if (actor.IsFailure) return Result<ResourceCardType>.Failure(actor.Error);

        var result = game.ResolveRobber(actor.Value, command.NewRobberHexCoord, command.VictimId);
        if (result.IsFailure) return Result<ResourceCardType>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);

        return saved ? Result<ResourceCardType>.Success(result.Value) : Result<ResourceCardType>.Failure(new Error("Persistence", "Failed to save game state"));
    }
}
