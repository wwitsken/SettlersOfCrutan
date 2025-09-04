using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public Result<PlayerId> EndTurn(PlayerId playerId)
    {
        if (CurrentPlayerId() != playerId) return Result.Failure<PlayerId>(DomainErrors.DomainError.WrongTurn);

        switch (GamePhase)
        {
            case GamePhase.Setup:
                if (PlayerIndex < Players.Count - 1 && PlayerDirection == PlayerDirection.Clockwise)
                    PlayerIndex++;
                else if (PlayerIndex == Players.Count - 1 && PlayerDirection == PlayerDirection.Clockwise)
                    PlayerDirection = PlayerDirection.CounterClockwise;
                else if (PlayerIndex > 0 && PlayerDirection == PlayerDirection.CounterClockwise)
                    PlayerIndex--;
                else if (PlayerIndex == 0 && PlayerDirection == PlayerDirection.CounterClockwise)
                {
                    GamePhase = GamePhase.RollDice;
                    PlayerIndex = 0;
                    Round = 1;
                }
                else return Result.Failure<PlayerId>(new Error("EndTurn", "Invalid player index or direction"));
                break;
            case GamePhase.TradeBuild:
                GamePhase = GamePhase.RollDice;
                PlayerIndex = (PlayerIndex + 1) % Players.Count;
                if (PlayerIndex == 0) Round++;
                break;
            default:
                return Result.Failure<PlayerId>(new Error("EndTurn", "Invalid game phase"));
        }

        AddDomainEvent(new PlayerTurnEndedDomainEvent(Id, playerId, CurrentPlayerId()));

        return Result.Success(CurrentPlayerId());
    }

    public Result<(int, int)> RollAndResolveProduction(PlayerId playerId)
    {
        if (CurrentPlayerId() != playerId) return Result.Failure<(int, int)>(DomainErrors.DomainError.WrongTurn);

        var rng = new Random();
        int d1 = rng.Next(1, 7);
        int d2 = rng.Next(1, 7);
        int total = d1 + d2;

        AddDomainEvent(new DiceRolledDomainEvent(Id, d1, d2));

        if (total == 7)
        {
            var playersNeedingToDiscard = SetTurnDiscardRequirements();
            if (playersNeedingToDiscard > 0)
            {
                GamePhase = GamePhase.DiscardHalf;
                AddDomainEvent(new DiscardHalfStartedDomainEvent(Id, PlayersNeedingToDiscardHalf));
            }
            else
            {
                GamePhase = GamePhase.ResolveRobber;
                AddDomainEvent(new RobPlayerStartedDomainEvent(Id));
            }
        }
        else
        {
            List<Hex> producingHexes = [.. Board.Hexes.Where(h => h.NumberToken == total && !h.HasRobber)];

            Dictionary<PlayerId, List<ResourceAmount>> distResources = [];
            foreach (var hex in producingHexes)
            {
                var pops = Board.PopulationCenters.Where(p => p.VertexCoordinate.HexCoords().Any(h => h == hex.Coordinate));


                foreach (var pop in pops)
                {
                    var owner = Players.FirstOrDefault(p => p.Id == pop.PlayerOwner);
                    if (owner is null) continue;

                    int amountToGive = pop.Level switch
                    {
                        PopulationCenterLevel.Settlement => 1,
                        PopulationCenterLevel.City => 2,
                        _ => 0
                    };

                    if (amountToGive > 0)
                    {
                        owner.ResourceBag ??= new ResourceBag();
                        owner.ResourceBag.AddResource(hex.Resource, amountToGive);
                        distResources.TryAdd(owner.Id, []);
                        distResources[owner.Id].Add(new ResourceAmount(hex.Resource, amountToGive));
                    }
                }
            }

            AddDomainEvent(new ResourcesDistributedDomainEvent(Id, distResources));

            GamePhase = GamePhase.TradeBuild;

            AddDomainEvent(new TradeBuildStartedDomainEvent(Id, CurrentPlayerId()));
        }

        return Result.Success((d1, d2));
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

    public Result<Nothing> DiscardHalf(PlayerId playerId, List<ResourceAmount> discards)
    {
        var req = DiscardHalfRequirements.FirstOrDefault(r => r.PlayerId.Equals(playerId));
        if (req is null) return Result.Failure(new Error("PlayerNotRequired", "Player not required to discard"));

        if (discards is null || discards.Count == 0) return Result.Failure(new Error("InvalidDiscardsPayload", "Invalid discards payload"));

        int toDiscardTotal = discards.Sum(kvp => Math.Max(0, kvp.Quantity));
        if (toDiscardTotal != req.ResourceAmount) return Result.Failure(new Error("IncorrectDiscardAmount", "Incorrect discard amount"));

        var player = Players.FirstOrDefault(p => p.Id.Equals(playerId));
        if (player is null) return Result.Failure(DomainErrors.DomainError.NotFound);
        player.ResourceBag ??= new ResourceBag();

        foreach (var (type, amount) in discards)
        {
            if (amount < 0)
                return Result.Failure(new Error("NegativeDiscardAmount", "Negative discard amount"));
            if (!player.ResourceBag.HasAtLeast(type, amount))
                return Result.Failure(new Error("InsufficientCards", "Insufficient cards to discard"));
        }
        foreach (var (type, amount) in discards)
        {
            player.ResourceBag.SubtractResource(type, amount);
        }

        DiscardHalfRequirements.Remove(req);
        AddDomainEvent(new PlayerDiscardedHalfDomainEvent(Id, playerId, discards));

        if (DiscardHalfRequirements.Count == 0)
        {
            GamePhase = GamePhase.ResolveRobber;
            AddDomainEvent(new DiscardHalfCompleteDomainEvent(Id));
        }

        return Result.Success();
    }
}
