using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Behaviors;
internal static class GameWinEvaluatorDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> inner,
        IGameRepository gameRepository,
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

            WinConditionEvaluator.EvaluateAndTransition(game);
            if (game.GamePhase == GamePhase.GameEnd)
            {
                var savedEnd = await gameRepository.SaveAsync(game, ct);
                if (!savedEnd)
                    logger.LogWarning("GameEnd persistence lost CAS for {GameId}; next action will retry win detection.", game.Id);
            }

            return res;
        }
    }

    internal sealed class CommandHandler<TCommand>(
        ICommandHandler<TCommand> inner,
        IGameRepository gameRepository,
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

            WinConditionEvaluator.EvaluateAndTransition(game);
            if (game.GamePhase == GamePhase.GameEnd)
            {
                var savedEnd = await gameRepository.SaveAsync(game, ct);
                if (!savedEnd)
                    logger.LogWarning("GameEnd persistence lost CAS for {GameId}; next action will retry win detection.", game.Id);
            }

            return res;
        }
    }
}
