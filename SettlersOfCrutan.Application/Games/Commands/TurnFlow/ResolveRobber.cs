using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.TurnFlow;

public record ResolveRobberCommand(GameId GameId, PlayerId PlayerId, HexCoord NewRobberHexCoord, PlayerId VictimId) : ICommand<ResourceCardType>;

public sealed class ResolveRobberCommandHandler(IGameRepository gameRepository) : ICommandHandler<ResolveRobberCommand, ResourceCardType>
{
    private readonly IGameRepository _gameRepository = gameRepository;

    public async Task<Result<ResourceCardType>> Handle(ResolveRobberCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<ResourceCardType>.Failure(DomainError.NotFound);

        var result = game.ResolveRobber(command.PlayerId, command.NewRobberHexCoord, command.VictimId);
        if (result.IsFailure) return Result<ResourceCardType>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        return saved ? Result<ResourceCardType>.Success(result.Value) : Result<ResourceCardType>.Failure(DomainError.InvalidOperation);
    }
}
