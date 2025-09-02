using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Generation;

namespace SettlersOfCrutan.Application.Games;

public record GenerateBoardCommand(string GameName, string[] UserIds);
public sealed class GenerateBoardCommandHandler
{
    private readonly IGameRepository _gameRepository;
    private readonly IBoardGenerator _boardGenerator;

    public GenerateBoardCommandHandler(IGameRepository gameRepository, IBoardGenerator boardGenerator)
    {
        _gameRepository = gameRepository;
        _boardGenerator = boardGenerator;
    }

    public async Task<Result<GameId>> Handle(GenerateBoardCommand command, CancellationToken ct = default)
    {
        var gameResult = Game.CreateGame(command.GameName, command.UserIds, _boardGenerator);

        if (gameResult.IsFailure) return Result<GameId>.Failure(new DomainError("GameCreation", "Game could not be spawned"));

        var success = await _gameRepository.SaveAsync(gameResult.Value, ct);

        return success ? Result<GameId>.Success(gameResult.Value.Id) : Result<GameId>.Failure(DomainError.InvalidOperation);
    }
}
