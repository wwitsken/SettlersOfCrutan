using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public Result<PlayerId> StartGame(IDateTimeProvider clock, TimeSpan? turnDuration = null)
    {
        GamePhase = GamePhase.Setup;
        PlayerIndex = 0;
        Round = 1;
        TurnExpiresAt = turnDuration.HasValue ? clock.UtcNow.Add(turnDuration.Value) : null;
        AddDomainEvent(new GameStartedDomainEvent(Id, CurrentPlayerId()));
        return Result.Success(CurrentPlayerId());
    }

    public Result<PlayerId> EndTurn(PlayerId playerId, IDateTimeProvider clock, TimeSpan? turnDuration = null)
    {
        if (CurrentPlayerId() != playerId)
            return Result.Failure<PlayerId>(DomainErrors.DomainError.WrongTurn);

        // Only valid during Setup or TradeBuild phases
        return GamePhase switch
        {
            GamePhase.Setup => EndTurnDuringSetup(playerId, clock, turnDuration),
            GamePhase.TradeBuild => EndTurnDuringTradeBuild(playerId, clock, turnDuration),
            _ => Result.Failure<PlayerId>(new Error("EndTurn", "Invalid game phase"))
        };
    }

    public Result<(int, int)> RollAndResolveProduction(PlayerId playerId)
    {
        if (CurrentPlayerId() != playerId)
            return Result.Failure<(int, int)>(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.RollDice)
            return Result.Failure<(int, int)>(new Error("Roll", "Cannot roll in the current game phase"));

        int d1 = Random.Shared.Next(1, 7);
        int d2 = Random.Shared.Next(1, 7);
        int total = d1 + d2;

        if (total == 7)
        {
            HandleSevenRoll(d1, d2);
        }
        else
        {
            var distResources = DistributeProduction(total);
            HandleProductionEvents(d1, d2, distResources);
        }

        return Result.Success((d1, d2));
    }

    // ------------ HELPERS ------------ //
    private Result<PlayerId> EndTurnDuringSetup(PlayerId playerId, IDateTimeProvider clock, TimeSpan? turnDuration = null)
    {
        if (PlayerIndex < Players.Count - 1 && PlayerDirection == PlayerDirection.Clockwise)
        {
            PlayerIndex++;
        }
        else if (PlayerIndex == Players.Count - 1 && PlayerDirection == PlayerDirection.Clockwise)
        {
            PlayerDirection = PlayerDirection.CounterClockwise;
        }
        else if (PlayerIndex > 0 && PlayerDirection == PlayerDirection.CounterClockwise)
        {
            PlayerIndex--;
        }
        else if (PlayerIndex == 0 && PlayerDirection == PlayerDirection.CounterClockwise)
        {
            GamePhase = GamePhase.RollDice;
            PlayerIndex = 0;
            Round = 1;
        }
        else
        {
            return Result.Failure<PlayerId>(new Error("EndTurn", "Invalid player index or direction"));
        }

        TurnExpiresAt = turnDuration.HasValue ? clock.UtcNow.Add(turnDuration.Value) : null;

        AddDomainEvent(new PlayerTurnEndedDomainEvent(Id, playerId, CurrentPlayerId()));
        return Result.Success(CurrentPlayerId());
    }

    private Result<PlayerId> EndTurnDuringTradeBuild(PlayerId playerId, IDateTimeProvider clock, TimeSpan? turnDuration = null)
    {
        GamePhase = GamePhase.RollDice;
        PlayerIndex = (PlayerIndex + 1) % Players.Count;
        if (PlayerIndex == 0) Round++;

        TurnExpiresAt = turnDuration.HasValue ? clock.UtcNow.Add(turnDuration.Value) : null;

        AddDomainEvent(new PlayerTurnEndedDomainEvent(Id, playerId, CurrentPlayerId()));
        return Result.Success(CurrentPlayerId());
    }

    private void HandleSevenRoll(int d1, int d2)
    {
        int playersNeedingToDiscard = SetTurnDiscardRequirements();
        GamePhase = playersNeedingToDiscard > 0 ? GamePhase.DiscardHalf : GamePhase.ResolveRobber;

        AddDomainEvent(new DiceRolledDomainEvent(Id, d1, d2));
        if (playersNeedingToDiscard > 0)
            AddDomainEvent(new DiscardHalfStartedDomainEvent(Id, PlayersNeedingToDiscardHalf));
        else
            AddDomainEvent(new RobPlayerStartedDomainEvent(Id));
    }

    private Dictionary<PlayerId, List<ResourceAmount>> DistributeProduction(int diceTotal)
    {
        var distResources = new Dictionary<PlayerId, List<ResourceAmount>>();

        foreach (var hex in Board.Hexes.Where(h => h.NumberToken == diceTotal && !h.HasRobber))
        {
            foreach (var pop in Board.PopulationCenters.Where(p => p.VertexCoordinate.HexCoords().Contains(hex.Coordinate)))
            {
                int amountToGive = pop.Level switch
                {
                    PopulationCenterLevel.Settlement => 1,
                    PopulationCenterLevel.City => 2,
                    _ => 0
                };
                if (amountToGive <= 0) continue;

                distResources.TryAdd(pop.PlayerOwner, []);
                distResources[pop.PlayerOwner].Add(new ResourceAmount(hex.Resource, amountToGive));
            }
        }

        // Group all resources by type and sum quantities
        var groupedAmounts = distResources.Values
            .SelectMany(ra => ra)
            .GroupBy(ra => ra.Type)
            .Select(g => new ResourceAmount(g.Key, g.Sum(ra => ra.Quantity)))
            .ToList();

        // Remove resources if bank doesn't have enough
        foreach (var amount in groupedAmounts.Where(amount => !Bank.HasAtLeast(amount)))
        {
            foreach (var resources in distResources.Values)
                resources.RemoveAll(ra => ra.Type == amount.Type);
        }

        // Apply resources to players and bank
        foreach (var (playerId, resources) in distResources)
        {
            var totalToGive = resources
                .GroupBy(ra => ra.Type)
                .Select(g => new ResourceAmount(g.Key, g.Sum(ra => ra.Quantity)))
                .ToList();

            Players.First(p => p.Id == playerId).ResourceBag.ApplyResourceAmounts(totalToGive);
            Bank.ApplyResourceAmounts(totalToGive.Invert());
        }

        GamePhase = GamePhase.TradeBuild;

        return distResources;
    }

    private void HandleProductionEvents(int d1, int d2, Dictionary<PlayerId, List<ResourceAmount>> distResources)
    {
        AddDomainEvent(new DiceRolledDomainEvent(Id, d1, d2));
        AddDomainEvent(new ResourcesDistributedDomainEvent(Id, distResources));
        AddDomainEvent(new TradeBuildStartedDomainEvent(Id, CurrentPlayerId()));
    }

    private int SetTurnDiscardRequirements()
    {
        int playersNeedingToDiscard = 0;
        foreach (var player in Players)
        {
            player.ResourceBag ??= new ResourceBag();
            int totalResources = player.ResourceBag.TotalResourceCards;
            if (totalResources > 7)
            {
                int toDiscard = totalResources / 2;
                DiscardHalfRequirements.Add(new DiscardHalfRequirement
                {
                    PlayerId = player.Id,
                    ResourceAmount = toDiscard
                });
                playersNeedingToDiscard++;
            }
        }
        return playersNeedingToDiscard;
    }
}
