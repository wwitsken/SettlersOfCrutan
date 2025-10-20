using FluentValidation;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Generation;

namespace SettlersOfCrutan.Application.Games.Commands.Lifecycle;

public sealed class CreateGameCommandValidator : AbstractValidator<CreateGameCommand>
{
    public CreateGameCommandValidator()
    {
        RuleFor(c => c.GameName).NotEmpty();
        RuleForEach(c => c.UserIds).NotEmpty();
        RuleFor(c => c.UserIds).Must(u => u.Length > 2).WithMessage("At least 2 players are required to start a game");
        RuleFor(c => c.UserIds).Must(u => u.Length <= 4).WithMessage("A maximum of 4 players are allowed in a game");
    }
}

public record CreateGameCommand(string GameName, string[] UserIds, GameType GameType) : ICommand<GameId>;

public sealed class CreateGameCommandHandler(IGameRepository gameRepository, IBoardGenerator boardGenerator) : ICommandHandler<CreateGameCommand, GameId>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IBoardGenerator _boardGenerator = boardGenerator;

    public async Task<Result<GameId>> Handle(CreateGameCommand command, CancellationToken ct = default)
    {
        var shuffledUserIds = command.UserIds.OrderBy(_ => Random.Shared.Next()).ToArray();

        var result = Game.CreateGame(command.GameName, shuffledUserIds, _boardGenerator);
        if (result.IsFailure || result.Value is null)
            return Result<GameId>.Failure(result.Error);

        var game = result.Value;
        var saved = await _gameRepository.SaveAsync(game, ct);

        return saved ? Result<GameId>.Success(game.Id) : Result<GameId>.Failure(DomainError.InvalidOperation);
    }
}
