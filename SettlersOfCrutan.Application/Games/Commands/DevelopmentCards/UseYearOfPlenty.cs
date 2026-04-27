using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.DevelopmentCards;

public record UseYearOfPlentyCommandResult(ResourceCardType ResourceType1, ResourceCardType ResourceType2);
public record UseYearOfPlentyCommand(GameId GameId, ResourceCardType Resource1, ResourceCardType Resource2) : IGameCommand<UseYearOfPlentyCommandResult>;

public sealed class UseYearOfPlentyCommandHandler(
    IGameRepository gameRepository,
    ICurrentUser currentUser,
    ILogger<UseYearOfPlentyCommandHandler> logger) : ICommandHandler<UseYearOfPlentyCommand, UseYearOfPlentyCommandResult>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly ILogger<UseYearOfPlentyCommandHandler> _logger = logger;

    public async Task<Result<UseYearOfPlentyCommandResult>> Handle(UseYearOfPlentyCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<UseYearOfPlentyCommandResult>.Failure(DomainError.NotFound);

        var actor = GamePlayerResolution.ResolveActor(game, await _currentUser.UserId());
        if (actor.IsFailure) return Result<UseYearOfPlentyCommandResult>.Failure(actor.Error);

        var result = game.PlayYearOfPlenty(actor.Value, command.Resource1, command.Resource2);
        if (result.IsFailure) return Result<UseYearOfPlentyCommandResult>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);

        return saved
            ? Result<UseYearOfPlentyCommandResult>.Success(new(result.Value.t1, result.Value.t2))
            : Result<UseYearOfPlentyCommandResult>.Failure(new Error("Persistence", "Failed to save game state"));
    }
}
