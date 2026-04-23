using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Application.Games.DomainEventHandlers;

/// <summary>
/// Reacts to <see cref="PlayerWonDomainEvent"/> (dispatched by the repository after CAS success),
/// composes a <see cref="GameEndedDto"/> with the full final scoreboard (including hidden VP cards)
/// and broadcasts it over SignalR as <see cref="RealtimeEvents.GameEnded"/>.
/// </summary>
public sealed class PlayerWonDomainEventHandler(
    IGameRepository gameRepository,
    IUserRepository userRepository,
    IRealtimePublisher realtimePublisher,
    IDateTimeProvider clock,
    ILogger<PlayerWonDomainEventHandler> logger)
    : IDomainEventHandler<PlayerWonDomainEvent>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly IDateTimeProvider _clock = clock;
    private readonly ILogger<PlayerWonDomainEventHandler> _logger = logger;

    public async Task<Result<PlayerWonDomainEvent>> HandleAsync(
        PlayerWonDomainEvent domainEvent,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var game = await _gameRepository.GetAsync(domainEvent.GameId, ct);
        if (game is null) return Result.Failure<PlayerWonDomainEvent>(DomainError.NotFound);

        var winner = game.Players.FirstOrDefault(p => p.Id == domainEvent.WinnerPlayerId);
        if (winner is null) return Result.Failure<PlayerWonDomainEvent>(
            DomainError.PlayerNotFound(game.Id, domainEvent.WinnerPlayerId));

        var payload = BuildPayload(game, winner);

        var users = await _userRepository.ResolveUsersForRealtimeAsync(
            game.Players.Select(p => p.UserId),
            ct);

        if (users.Count == 0) return Result<PlayerWonDomainEvent>.Success(domainEvent);

        try
        {
            await _realtimePublisher.UpdateGameAsync(
                game.Id,
                users,
                _clock.UtcNow,
                RealtimeEvents.GameEnded,
                payload,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish GameEnded for GameId {GameId}", game.Id);
        }

        return Result<PlayerWonDomainEvent>.Success(domainEvent);
    }

    private static GameEndedDto BuildPayload(Game game, Player winner)
    {
        var buildingVp = GamePresentationScoring.BuildingVictoryPoints(game);
        var longestRoad = GamePresentationScoring.LongestRoadHolders(game);
        var largestArmy = GamePresentationScoring.LargestArmyHolders(game);

        var rows = game.Players
            .Select(p => ToRow(p, buildingVp, longestRoad, largestArmy))
            .ToList();

        var winnerRow = rows.First(r => r.PlayerId == winner.Id.Value);

        rows.Sort((a, b) => b.VictoryPoints.CompareTo(a.VictoryPoints));

        return new GameEndedDto(
            WinnerPlayerId: winner.Id.Value,
            WinnerUserId: winner.UserId.Value,
            WinnerDisplayName: winner.DisplayName,
            WinnerColor: winner.Color,
            WinnerVictoryPoints: winnerRow.VictoryPoints,
            FinalScores: rows);
    }

    private static FinalPlayerScoreDto ToRow(
        Player player,
        IReadOnlyDictionary<PlayerId, int> buildingVp,
        IReadOnlySet<PlayerId> longestRoad,
        IReadOnlySet<PlayerId> largestArmy)
    {
        var observable = GamePresentationScoring.ObservableVictoryPoints(
            buildingVp.GetValueOrDefault(player.Id, 0),
            longestRoad.Contains(player.Id),
            largestArmy.Contains(player.Id));
        var hidden = player.GetDevelopmentCards()
            .TryGetValue(Domain.Games.Resources.DevelopmentCardType.VictoryPoint, out var n) ? n : 0;

        return new FinalPlayerScoreDto(
            PlayerId: player.Id.Value,
            UserId: player.UserId.Value,
            DisplayName: player.DisplayName,
            PlayerColor: player.Color,
            VictoryPoints: observable + hidden,
            ObservableVictoryPoints: observable,
            HiddenVictoryPointCards: hidden,
            HasLongestRoad: longestRoad.Contains(player.Id),
            HasLargestArmy: largestArmy.Contains(player.Id));
    }
}
