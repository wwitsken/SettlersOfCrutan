using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.Trade;

public record MaritimeTrade3to1Command(GameId GameId, ResourceCardType DiscardResource, ResourceCardType RequestResource) : IGameCommand;

public sealed class MaritimeTrade3to1CommandHandler(
    IGameRepository gameRepository,
    ICurrentUser currentUser,
    ILogger<MaritimeTrade3to1CommandHandler> logger) : ICommandHandler<MaritimeTrade3to1Command>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly ILogger<MaritimeTrade3to1CommandHandler> _logger = logger;

    public async Task<Result<Nothing>> Handle(MaritimeTrade3to1Command command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<Nothing>.Failure(DomainError.NotFound);

        var actor = GamePlayerResolution.ResolveActor(game, await _currentUser.UserId());
        if (actor.IsFailure) return Result<Nothing>.Failure(actor.Error);

        var result = game.Maritime3to1Trade(actor.Value, command.DiscardResource, command.RequestResource);
        if (result.IsFailure) return Result<Nothing>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);

        return saved ? Result<Nothing>.Success() : Result<Nothing>.Failure(new Error("Persistence", "Failed to save game state"));
    }
}
