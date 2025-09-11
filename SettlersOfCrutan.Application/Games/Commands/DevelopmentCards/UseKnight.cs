using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Application.Games.Commands.DevelopmentCards;

public record UseKnightCommand(GameId GameId, PlayerId PlayerId, HexCoord NewRobberHexCoord, PlayerId VictimId) : ICommand<HexCoord>;

public sealed class UseKnightCommandHandler(IGameRepository gameRepository) : ICommandHandler<UseKnightCommand, HexCoord>
{
    private readonly IGameRepository _gameRepository = gameRepository;

    public async Task<Result<HexCoord>> Handle(UseKnightCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<HexCoord>.Failure(DomainError.NotFound);

        var result = game.PlayKnight(command.PlayerId, command.NewRobberHexCoord, command.VictimId);
        if (result.IsFailure) return Result<HexCoord>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        return saved ? Result<HexCoord>.Success(result.Value) : Result<HexCoord>.Failure(DomainError.InvalidOperation);
    }
}
