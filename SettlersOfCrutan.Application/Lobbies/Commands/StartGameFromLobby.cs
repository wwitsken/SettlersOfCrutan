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
        RuleFor(c => c.LobbyId.Value).NotEqual(Guid.Empty);
        RuleFor(c => c.GameType).IsInEnum();
    }
}

public record StartGameFromLobbyCommand(LobbyId LobbyId, GameType GameType, string? GameName) : ICommand<GameId>;

public sealed class StartGameFromLobbyCommandHandler(IGameRepository gameRepository,
                                                     IBoardGenerator boardGenerator,
                                                     ILobbyRepository lobbyRepository,
                                                     ICurrentUser currentUser,
                                                     IUserRepository userRepository,
                                                     IRealtimePublisher realtimePublisher,
                                                     IDateTimeProvider clock,
                                                     ILogger<StartGameFromLobbyCommandHandler> logger) : ICommandHandler<StartGameFromLobbyCommand, GameId>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IBoardGenerator _boardGenerator = boardGenerator;
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly IDateTimeProvider _clock = clock;
    private readonly ILogger<StartGameFromLobbyCommandHandler> _logger = logger;

    public async Task<Result<GameId>> Handle(StartGameFromLobbyCommand command, CancellationToken ct = default)
    {
        var lobby = await _lobbyRepository.GetAsync(command.LobbyId, ct);
        if (lobby is null) return Result<GameId>.Failure(DomainError.NotFound);

        var userId = await _currentUser.UserId();
        var canStart = lobby.CanStartGame(userId);
        if (canStart.IsFailure) return Result<GameId>.Failure(canStart.Error);

        var shuffledUserIds = lobby.Members
            .OrderBy(_ => Random.Shared.Next())
            .Select(m => m.UserId)
            .ToList();

        var result = Game.CreateGame(command.GameName ?? "Crutan Game", lobby.Id, shuffledUserIds, _boardGenerator);
        if (result.IsFailure) return Result<GameId>.Failure(result.Error);

        var game = result.Value;
        var now = _clock.UtcNow;
        foreach (var p in game.Players)
            p.JoinedAt ??= now;
        game.StartGame(_clock);

        var saved = await _gameRepository.SaveAsync(game, ct);

        if (saved)
        {
            try
            {
                var usersForRealtime = await _userRepository.ResolveUsersForRealtimeAsync(shuffledUserIds, ct);
                await _realtimePublisher.MoveFromLobbyToGameAsync(lobby.Id, game.Id, usersForRealtime, now, ct);

                var gameViews = GameDto.UserViewsFromGame(game);
                await _realtimePublisher.PublishGameStateToAllPlayersAsync(
                    _userRepository,
                    game.Id,
                    gameViews,
                    now,
                    RealtimeEvents.GameStateUpdated,
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish realtime for new game {GameId} from lobby {LobbyId}", game.Id, lobby.Id);
            }
        }

        return saved ? Result<GameId>.Success(game.Id) : Result<GameId>.Failure(DomainError.InvalidOperation);
    }
}
