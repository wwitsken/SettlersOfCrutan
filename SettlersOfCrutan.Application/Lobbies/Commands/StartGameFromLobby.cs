using FluentValidation;
using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Generation;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Lobbies.Commands;

public sealed class StartGameFromLobbyValidator : AbstractValidator<StartGameFromLobbyCommand>
{
    public StartGameFromLobbyValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.LobbyId).NotEmpty();
        RuleFor(c => c.GameType).IsInEnum();
    }
}

public record StartGameFromLobbyCommand(string UserId, Guid LobbyId, GameType GameType, string? GameName) : ICommand<GameId>;

public sealed class StartGameFromLobbyCommandHandler(IGameRepository gameRepository,
                                                     IBoardGenerator boardGenerator,
                                                     ILobbyRepository lobbyRepository,
                                                     IRealtimePublisher realtimePublisher,
                                                     IDateTimeProvider clock,
                                                     ILogger<ChangeReadyStatusCommandHandler> logger) : ICommandHandler<StartGameFromLobbyCommand, GameId>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IBoardGenerator _boardGenerator = boardGenerator;
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly IDateTimeProvider _clock = clock;
    private readonly ILogger<ChangeReadyStatusCommandHandler> _logger = logger;

    public async Task<Result<GameId>> Handle(StartGameFromLobbyCommand command, CancellationToken ct = default)
    {
        var lobby = await _lobbyRepository.GetAsync(new LobbyId() { Value = command.LobbyId }, ct);
        if (lobby is null) return Result<GameId>.Failure(DomainError.NotFound);

        var canStart = lobby.CanStartGame(command.UserId);
        if (canStart.IsFailure) return Result<GameId>.Failure(canStart.Error);

        var shuffledUserIds = lobby.Members
            .OrderBy(_ => Random.Shared.Next())
            .Select(m => m.UserId)
            .OfType<string>()
            .ToArray();

        var result = Game.CreateGame(command.GameName ?? "Crutan Game", lobby.Id, shuffledUserIds, _boardGenerator);
        if (result.IsFailure) return Result<GameId>.Failure(result.Error);

        var game = result.Value;
        var saved = await _gameRepository.SaveAsync(game, ct);

        var now = _clock.UtcNow;

        if (saved)
        {
            try
            {
                await _realtimePublisher.MoveFromLobbyToGameAsync(lobby.Id, game.Id, shuffledUserIds, now, ct);

                var gameViews = GameDto.UserViewsFromGame(game);
                var publishTasks = gameViews.Select(kvp =>
                    _realtimePublisher.UpdateGameAsync(
                        game.Id,
                        kvp.Key,
                        now,
                        RealtimeEvents.GameStateUpdated,
                        kvp.Value,
                        ct));
                await Task.WhenAll(publishTasks);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish realtime for new game {GameId} from lobby {LobbyId}", game.Id, lobby.Id);
            }
        }

        return saved ? Result<GameId>.Success(game.Id) : Result<GameId>.Failure(DomainError.InvalidOperation);
    }
}
