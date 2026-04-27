using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.DevelopmentCards;

public record UseMonopolyCommand(GameId GameId, ResourceCardType ResourceType) : IGameCommand<int>;

public sealed class UseMonopolyCommandHandler(
    IGameRepository gameRepository,
    ICurrentUser currentUser) : ICommandHandler<UseMonopolyCommand, int>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<int>> Handle(UseMonopolyCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<int>.Failure(DomainError.NotFound);

        var actor = GamePlayerResolution.ResolveActor(game, await _currentUser.UserId());
        if (actor.IsFailure) return Result<int>.Failure(actor.Error);

        var result = game.PlayMonopoly(actor.Value, command.ResourceType);
        if (result.IsFailure) return Result<int>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);

        return saved ? Result<int>.Success(result.Value) : Result<int>.Failure(new Error("Persistence", "Failed to save game state"));
    }
}
