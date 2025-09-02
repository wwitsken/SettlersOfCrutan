using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Generation;

namespace SettlersOfCrutan.Application.Games;

public record CreateGameCommand(string GameName, string[] UserIds);

public sealed class CreateGameCommandHandler
{
    private readonly IGameRepository _gameRepository;
    private readonly IBoardGenerator _boardGenerator;

    public CreateGameCommandHandler(IGameRepository gameRepository, IBoardGenerator boardGenerator)
    {
        _gameRepository = gameRepository;
        _boardGenerator = boardGenerator;
    }

    public async Task<Result<GameId>> Handle(CreateGameCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.GameName))
            return Result<GameId>.Failure(new Error("Validation", "Game name is required"));

        var result = Game.CreateGame(command.GameName, command.UserIds, _boardGenerator);
        if (result.IsFailure || result.Value is null)
            return Result<GameId>.Failure(DomainError.InvalidOperation);

        var game = result.Value;
        var saved = await _gameRepository.SaveAsync(game, ct).ConfigureAwait(false);
        return saved ? Result<GameId>.Success(game.Id) : Result<GameId>.Failure(DomainError.InvalidOperation);
    }
}
