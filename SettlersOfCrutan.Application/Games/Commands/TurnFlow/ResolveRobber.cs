using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Games;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.TurnFlow;

public record ResolveRobberCommand(GameId GameId, HexCoord NewRobberHexCoord, PlayerId? VictimId) : ICommand<ResourceCardType>;

public sealed class ResolveRobberCommandHandler(
    IGameRepository gameRepository,
    ICurrentUser currentUser,
    IUserRepository userRepository,
    IRealtimePublisher realtimePublisher,
    IDateTimeProvider clock,
    ILogger<ResolveRobberCommandHandler> logger) : ICommandHandler<ResolveRobberCommand, ResourceCardType>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly IDateTimeProvider _clock = clock;
    private readonly ILogger<ResolveRobberCommandHandler> _logger = logger;

    public async Task<Result<ResourceCardType>> Handle(ResolveRobberCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<ResourceCardType>.Failure(DomainError.NotFound);

        var actor = GamePlayerResolution.ResolveActor(game, await _currentUser.UserId());
        if (actor.IsFailure) return Result<ResourceCardType>.Failure(actor.Error);

        var result = game.ResolveRobber(actor.Value, command.NewRobberHexCoord, command.VictimId);
        if (result.IsFailure) return Result<ResourceCardType>.Failure(result.Error);

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

        return saved ? Result<ResourceCardType>.Success(result.Value) : Result<ResourceCardType>.Failure(DomainError.InvalidOperation);
    }
}
