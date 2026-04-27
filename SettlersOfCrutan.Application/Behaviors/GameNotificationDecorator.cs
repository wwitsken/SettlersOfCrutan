using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Application.Behaviors;
internal static class GameNotificationDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> inner,
        IGameRepository gameRepository,
        IUserRepository userRepository,
        IDateTimeProvider clock,
        IRealtimePublisher realtimePublisher,
        ILogger<ICommandHandler<TCommand, TResponse>> logger)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : IGameCommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct = default)
        {
            var res = await inner.Handle(command, ct);

            if (res.IsFailure) return res;

            var game = await gameRepository.GetAsync(command.GameId, ct);

            if (game is null)
            {
                logger.LogWarning("Failed to publish GameStateUpdated for GameId {GameId}: Game not found.", command.GameId);
                return res;
            }

            var now = clock.UtcNow;
            var userViews = GameDto.UserViewsFromGame(game);

            try
            {
                await realtimePublisher.PublishGameStateToAllPlayersAsync(
                    userRepository,
                    game.Id,
                    userViews,
                    now,
                    RealtimeEvents.GameStateUpdated,
                    ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to publish GameStateUpdated for GameId {GameId}: Internal exception.", game.Id);
            }

            return res;
        }
    }

    internal sealed class CommandHandler<TCommand>(
        ICommandHandler<TCommand> inner,
        IGameRepository gameRepository,
        IUserRepository userRepository,
        IDateTimeProvider clock,
        IRealtimePublisher realtimePublisher,
        ILogger<ICommandHandler<TCommand>> logger)
        : ICommandHandler<TCommand>
        where TCommand : IGameCommand
    {
        public async Task<Result<Nothing>> Handle(TCommand command, CancellationToken ct = default)
        {
            var res = await inner.Handle(command, ct);

            if (res.IsFailure) return res;

            var game = await gameRepository.GetAsync(command.GameId, ct);

            if (game is null)
            {
                logger.LogWarning("Failed to publish GameStateUpdated for GameId {GameId}: Game not found.", command.GameId);
                return res;
            }

            var now = clock.UtcNow;
            var userViews = GameDto.UserViewsFromGame(game);

            try
            {
                await realtimePublisher.PublishGameStateToAllPlayersAsync(
                    userRepository,
                    game.Id,
                    userViews,
                    now,
                    RealtimeEvents.GameStateUpdated,
                    ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to publish GameStateUpdated for GameId {GameId}: Internal exception.", game.Id);
            }

            return res;
        }
    }
}
