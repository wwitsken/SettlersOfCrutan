using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Games;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.TurnFlow;

public record DiscardHalfCommand(GameId GameId, List<ResourceCardAmount> Discards) : ICommand;

public sealed class DiscardHalfCommandHandler(
    IGameRepository gameRepository,
    ICurrentUser currentUser,
    IUserRepository userRepository,
    IRealtimePublisher realtimePublisher,
    IDateTimeProvider clock,
    ILogger<DiscardHalfCommandHandler> logger) : ICommandHandler<DiscardHalfCommand>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly IDateTimeProvider _clock = clock;
    private readonly ILogger<DiscardHalfCommandHandler> _logger = logger;

    public async Task<Result<Nothing>> Handle(DiscardHalfCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<Nothing>.Failure(DomainError.NotFound);

        var actor = GamePlayerResolution.ResolveActor(game, await _currentUser.UserId());
        if (actor.IsFailure) return Result<Nothing>.Failure(actor.Error);

        var result = game.DiscardHalf(actor.Value, command.Discards);
        if (result.IsFailure) return Result<Nothing>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);

        if (saved)
        {
            var now = _clock.UtcNow;
            var userViews = GameDto.UserViewsFromGame(game);

            try
            {
                await _realtimePublisher.PublishGameStateToAllPlayersAsync(
                    _userRepository,
                    game.Id,
                    userViews,
                    now,
                    RealtimeEvents.GameStateUpdated,
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish GameStateUpdated for GameId {GameId}", game.Id);
            }
        }

        return saved ? Result<Nothing>.Success() : Result<Nothing>.Failure(DomainError.InvalidOperation);
    }
}
