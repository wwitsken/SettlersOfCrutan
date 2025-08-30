using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Generation;

namespace SettlersOfCrutan.Application.Games;

public record GenerateBoardCommand(string[] UserIds);
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
        Board board = _boardGenerator.Generate(StandardBoardConfigurations.DefaultBaseGame, Environment.TickCount);

        var game = new Game() { Board = board };

        var players = command.UserIds.Select((id, index) => Player.Create(index, id)).ToList();

        game.Players = players;

        var success = await _gameRepository.SaveAsync(game, ct);

        return success ? Result<GameId>.Success(game.Id) : Result<GameId>.Failure(DomainError.InvalidOperation);
    }
}
