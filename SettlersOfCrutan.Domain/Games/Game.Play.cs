using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications;
using EndTurnSpecs = SettlersOfCrutan.Domain.Specifications.EndTurn;
using RollSpecs = SettlersOfCrutan.Domain.Specifications.RollAndResolveProduction;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    private static readonly ISpecification<EndTurnSpecs.EndTurnContext>[] _endTurnSpecifications =
    [
        new EndTurnSpecs.GameMustBeInSetupOrTradeBuild(),
        new EndTurnSpecs.MustBeCurrentPlayerTurn(),
        new EndTurnSpecs.DuringSetupPhasePlayerMustHavePlacedInitialSettlementAndRoad()
    ];

    public Result<PlayerId> EndTurn(PlayerId playerId, IDateTimeProvider clock, TimeSpan? turnDuration = null)
    {
        var context = new EndTurnSpecs.EndTurnContext(GamePhase, Round, PlayerDirection,
            [.. Board.Roads.Where(r => r.OwnerId == playerId)],
            [.. Board.PopulationCenters.Where(r => r.OwnerId == playerId)], CurrentPlayerId(), playerId);

        foreach (var spec in _endTurnSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure)
                return Result.Failure<PlayerId>(result.Error);
        }

        return GamePhase switch
        {
            GamePhase.Setup => EndTurnDuringSetup(playerId, clock, turnDuration),
            GamePhase.TradeBuild => EndTurnDuringTradeBuild(playerId, clock, turnDuration),
            _ => throw new InvalidOperationException("Invalid game phase for ending turn")
        };
    }

    private static readonly ISpecification<RollSpecs.RollAndResolveProductionContext>[] RollAndResolveProductionSpecifications =
    [
        new RollSpecs.GameMustBeInRollDicePhase(),
        new RollSpecs.MustBeCurrentPlayerTurn()
    ];

    public Result<(int, int)> RollAndResolveProduction(PlayerId playerId)
    {
        var context = new RollSpecs.RollAndResolveProductionContext(GamePhase, CurrentPlayerId(), playerId);

        foreach (var spec in RollAndResolveProductionSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure)
                return Result.Failure<(int, int)>(result.Error);
        }

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
            return Result.Failure<PlayerId>(DomainError.InvalidEndTurn);
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

    private Dictionary<PlayerId, List<ResourceCardAmount>> DistributeProduction(int diceTotal)
    {
        var distResources = new Dictionary<PlayerId, List<ResourceCardAmount>>();

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

                distResources.TryAdd(pop.OwnerId, []);
                distResources[pop.OwnerId].Add(new ResourceCardAmount(hex.Resource, amountToGive));
            }
        }

        var groupedAmounts = distResources.Values
            .SelectMany(ra => ra)
            .GroupBy(ra => ra.Type)
            .Select(g => new ResourceCardAmount(g.Key, g.Sum(ra => ra.Quantity)))
            .ToList();

        foreach (var amount in groupedAmounts.Where(amount => !BankResourceHand.HasAtLeast(amount)))
        {
            foreach (var resources in distResources.Values)
                resources.RemoveAll(ra => ra.Type == amount.Type);
        }

        foreach (var (pid, resources) in distResources)
        {
            var totalToGive = resources
                .GroupBy(r => r.Type)
                .Select(g => new ResourceCardAmount(g.Key, g.Sum(x => x.Quantity)))
                .ToList();

            var player = Players.First(p => p.Id == pid);
            foreach (var ra in totalToGive)
            {
                player.AddResource(ra.Type, ra.Quantity);
                BankResourceHand.Subtract(ra.Type, ra.Quantity);
            }
        }

        GamePhase = GamePhase.TradeBuild;
        return distResources;
    }

    private void HandleProductionEvents(int d1, int d2, Dictionary<PlayerId, List<ResourceCardAmount>> distResources)
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
            int totalResources = player.TotalResources;
            if (totalResources > 7)
            {
                int toDiscard = totalResources / 2;
                _discardHalfRequirements.Add(new DiscardHalfRequirement
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
