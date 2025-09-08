using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Generation;

namespace SettlersOfCrutan.Application.Games.Commands.Lifecycle;

public record CreateGameCommand(string GameName, string[] UserIds, GameType GameType) : ICommand<GameId>;

public sealed class CreateGameCommandHandler(IGameRepository gameRepository, IBoardGenerator boardGenerator) : ICommandHandler<CreateGameCommand, GameId>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IBoardGenerator _boardGenerator = boardGenerator;

    public async Task<Result<GameId>> Handle(CreateGameCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.GameName))
            return Result<GameId>.Failure(new Error("Validation", "Game name is required"));

        if (command.GameType != GameType.BaseGame)
            return Result<GameId>.Failure(new Error("Validation", "Unsupported game type"));

        if (command.GameType == GameType.BaseGame && (command.UserIds.Length < 2 || command.UserIds.Length > 4))
            return Result<GameId>.Failure(new Error("Validation", "Base game requires 2 to 4 players"));

        var shuffledUserIds = command.UserIds.OrderBy(_ => Random.Shared.Next()).ToArray();

        var result = Game.CreateGame(command.GameName, shuffledUserIds, _boardGenerator);
        if (result.IsFailure || result.Value is null)
            return Result<GameId>.Failure(DomainError.InvalidOperation);

        var game = result.Value;
        var saved = await _gameRepository.SaveAsync(game, ct).ConfigureAwait(false);
        return saved ? Result<GameId>.Success(game.Id) : Result<GameId>.Failure(DomainError.InvalidOperation);
    }
}
