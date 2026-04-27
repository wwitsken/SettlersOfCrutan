using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.Commands.Trade;

public record AcceptTradeCommand(GameId GameId, TradeOfferId TradeOfferId) : IGameCommand;

public sealed class AcceptTradeCommandHandler(
    IGameRepository gameRepository,
    ICurrentUser currentUser,
    ILogger<AcceptTradeCommandHandler> logger) : ICommandHandler<AcceptTradeCommand>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly ILogger<AcceptTradeCommandHandler> _logger = logger;

    public async Task<Result<Nothing>> Handle(AcceptTradeCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<Nothing>.Failure(DomainError.NotFound);

        var actor = GamePlayerResolution.ResolveActor(game, await _currentUser.UserId());
        if (actor.IsFailure) return Result<Nothing>.Failure(actor.Error);

        var result = game.AcceptTrade(actor.Value, command.TradeOfferId);
        if (result.IsFailure) return Result<Nothing>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);

        return saved ? Result<Nothing>.Success() : Result<Nothing>.Failure(new Error("Persistence", "Failed to save game state"));
    }
}
