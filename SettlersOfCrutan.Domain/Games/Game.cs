using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Coordinates;

namespace SettlersOfCrutan.Domain.Games;

public record GameId : BaseId<Guid>;
public class Game : AggregateRoot<GameId>
{
    public override GameId Id { get; init; } = new() { Value = Guid.NewGuid() };
    public string Name { get; set; }

    private List<Player> _players = [];
    public List<Player> Players
    {
        get => [.. _players.OrderBy(p => p.Id.Value)];
        set => _players = value;
    }
    public GamePhase GamePhase { get; set; } = GamePhase.Setup1;
    public TurnDetails CurrentTurnDetails { get; set; } = new TurnDetails() { Status = TurnStatus.Active, Step = TurnStep.BuildInitial };
    public int Round { get; set; } = 1;
    public int PlayerIndex { get; set; } = 0;
    public Board Board { get; set; }

    // Turn state
    public DateTimeOffset? TurnExpiresAt => CurrentTurnDetails?.ExpiresAt;
    public PlayerId CurrentPlayerId() => Players[PlayerIndex].Id;

    public void NextPlayer()
    {
        switch (GamePhase)
        {
            case GamePhase.Setup1:
                if (PlayerIndex < Players.Count - 1)
                    PlayerIndex++;
                else
                    GamePhase = GamePhase.Setup2;
                break;

            case GamePhase.Setup2:
                if (PlayerIndex > 0)
                    PlayerIndex--;
                else
                {
                    GamePhase = GamePhase.Normal;
                    PlayerIndex = 0;
                    Round = 1;
                }
                break;

            default:
                PlayerIndex = (PlayerIndex + 1) % Players.Count;
                if (PlayerIndex == 0) Round++;
                break;
        }
    }

    public Result<(PopulationCenter, Road)> PlaceInitial(PlayerId playerId, Vertex settlementVertex, Edge roadEdge)
    {
        return Board.PlaceInitialSettlementAndRoad(playerId, settlementVertex, roadEdge);
    }

    public Result<int> Roll(Game game)
    {
        if (CurrentTurnDetails is null || CurrentTurnDetails.Status != TurnStatus.Active)
            return Result.Failure<int>(DomainErrors.DomainError.InvalidOperation);
        if (CurrentTurnDetails.Step != TurnStep.Roll)
            return Result.Failure<int>(DomainErrors.DomainError.InvalidOperation);

        var rng = new Random();
        int d1 = rng.Next(1, 7);
        int d2 = rng.Next(1, 7);
        int total = d1 + d2;

        if (total == 7)
        {
            var playersNeedingToDiscard = SetTurnDiscardRequirements();
            if (playersNeedingToDiscard.IsSuccess && playersNeedingToDiscard.Value > 0)
            {
                CurrentTurnDetails.AdvanceTo(TurnStep.DiscardHalf);
            }
            else
            {
                CurrentTurnDetails.AdvanceTo(TurnStep.RobPlayer);
            }
        }
        else
        {
            CurrentTurnDetails.AdvanceTo(TurnStep.ResolveProduction);
        }

        BumpVersion();
        return Result.Success(total);
    }

    public Result<Nothing> DiscardHalf(PlayerId playerId, IReadOnlyDictionary<ResourceType, int> discards)
    {
        var turn = CurrentTurnDetails;
        if (turn.Status != TurnStatus.Active || turn.Step != TurnStep.DiscardHalf)
            return Result.Failure(new Error("NotDiscardHalf", "DiscardHalf not active"));

        var req = turn.DiscardHalfRequirements.FirstOrDefault(r => r.PlayerId.Equals(playerId));
        if (req is null)
            return Result.Failure(new Error("PlayerNotRequired", "Player not required to discard"));

        if (discards is null || discards.Count == 0)
            return Result.Failure(new Error("InvalidDiscardsPayload", "Invalid discards payload"));

        int toDiscardTotal = discards.Sum(kvp => Math.Max(0, kvp.Value));
        if (toDiscardTotal != req.ResourceAmount)
            return Result.Failure(new Error("IncorrectDiscardAmount", "Incorrect discard amount"));

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

        CurrentTurnDetails.DiscardHalfRequirements.Remove(req);
        BumpVersion();
        return Result.Success();
    }

    public Result<int> SetTurnDiscardRequirements()
    {
        if (CurrentTurnDetails.Status != TurnStatus.Active || CurrentTurnDetails.Step != TurnStep.DiscardHalf)
            return Result<int>.Failure(new Error("NotDiscardHalf", "DiscardHalf not active"));
        if (CurrentTurnDetails.DiscardHalfRequirements.Count > 0)
            return Result.Success(0);

        int playersNeedingToDiscard = 0;
        foreach (var player in Players)
        {
            player.ResourceBag ??= new ResourceBag();
            int totalResources = player.ResourceBag.TotalResourceCards;
            if (totalResources > 7)
            {
                int toDiscard = totalResources / 2;
                CurrentTurnDetails.DiscardHalfRequirements.Add(new DiscardHalfRequirement
                {
                    PlayerId = player.Id,
                    ResourceAmount = toDiscard
                });
                playersNeedingToDiscard++;
            }
        }
        BumpVersion();
        return Result.Success(playersNeedingToDiscard);
    }

    public Result<Nothing> MoveRobberAndRobPlayer(HexCoord newRobberHex, PlayerId playerId)
    {
        if (CurrentTurnDetails is null || CurrentTurnDetails.Status != TurnStatus.Active)
            return Result.Failure(DomainErrors.DomainError.InvalidOperation);
        if (CurrentTurnDetails.Step != TurnStep.RobPlayer)
            return Result.Failure(DomainErrors.DomainError.InvalidOperation);

        var moveResult = Board.MoveRobber(newRobberHex);
        if (moveResult.IsFailure) return Result.Failure(moveResult.Error);

        if (!Board.IsPlayerExposedToHex(newRobberHex, playerId)) return Result.Failure(DomainErrors.DomainError.ValidationFailed);

        CurrentTurnDetails.AdvanceTo(TurnStep.TradeBuild);
        BumpVersion();
        return Result.Success();
    }

    public Result<Nothing> ResolveProduction(int rolledNumber)
    {
        if (CurrentTurnDetails is null || CurrentTurnDetails.Status != TurnStatus.Active)
            return Result.Failure(DomainErrors.DomainError.InvalidOperation);
        if (CurrentTurnDetails.Step != TurnStep.ResolveProduction)
            return Result.Failure(DomainErrors.DomainError.InvalidOperation);

        if (rolledNumber is < 2 or > 12 || rolledNumber == 7)
            return Result.Failure(DomainErrors.DomainError.ValidationFailed);

        var producingHexes = Board.Hexes.Where(h => h.NumberToken == rolledNumber && h.Resource != ResourceType.Desert);
        foreach (var hex in producingHexes)
        {
            foreach (var pc in Board.PopulationCenters)
            {
                if (pc.VertexCoordinate.HexCoord1.Equals(hex.Coordinate)
                    || pc.VertexCoordinate.HexCoord2.Equals(hex.Coordinate)
                    || pc.VertexCoordinate.HexCoord3.Equals(hex.Coordinate))
                {
                    var owner = Players.FirstOrDefault(p => p.Id.Equals(pc.PlayerOwner));
                    if (owner is null) continue;
                    owner.ResourceBag ??= new ResourceBag();
                    int amount = pc.Level == PopulationCenterLevel.City ? 2 : 1;
                    owner.ResourceBag.AddResource(hex.Resource, amount);
                }
            }
        }

        CurrentTurnDetails.AdvanceTo(TurnStep.TradeBuild);
        BumpVersion();
        return Result.Success();
    }

    public Result<Nothing> BuildRoad(Game game, PlayerId playerId, Edge edge)
    {
        // Ensure the player has enough resources (1 Brick, 1 Lumber) & 1 Road piece
        // Try putting the road down on the board
        // Deduct resources and road piece from player if successful
        throw new NotImplementedException();
    }

    public Result<Nothing> BuildSettlement(Game game, PlayerId playerId, Vertex vertex)
    {
        throw new NotImplementedException();
    }

    public Result<Nothing> BuildCity(Game game, PlayerId playerId, Vertex vertex)
    {
        throw new NotImplementedException();
    }

    public Result<Nothing> MakeMaritimeTrade()
    {
        throw new NotImplementedException();
    }

    public Result<Nothing> StartGame(IDateTimeProvider clock)
    {
        NextPlayer();
        CurrentTurnDetails = new TurnDetails() { Status = TurnStatus.Active, Step = TurnStep.BuildInitial };
        CurrentTurnDetails.Start(clock, default);
        BumpVersion();
        return Result.Success();
    }

    public Result<Nothing> EndTurnAndStartNextTurn(IDateTimeProvider clock, TimeSpan? turnDuration = null)
    {
        NextPlayer();
        CurrentTurnDetails = new TurnDetails()
        { Step = GamePhase == GamePhase.Normal ? TurnStep.Roll : TurnStep.BuildInitial, Status = TurnStatus.Active };
        CurrentTurnDetails.Start(clock, turnDuration);
        BumpVersion();
        return Result.Success();
    }

    public Result<Nothing> AdvanceTurnIfExpired(IDateTimeProvider clock)
    {
        if (CurrentTurnDetails.Status != TurnStatus.Active) return Result.Success();
        if (TurnExpiresAt.HasValue && TurnExpiresAt.Value <= clock.UtcNow)
        {
            CurrentTurnDetails.MarkTimedOut();
            EndTurnAndStartNextTurn(clock);
            // TODO: AddDomainEvent(new TurnAdvancedEvent(Id, CurrentTurn.PlayerId));
        }
        return Result.Success();
    }

    public List<PlayerId> PlayersNeedingToDiscardHalf => [.. CurrentTurnDetails.DiscardHalfRequirements.Select(r => r.PlayerId)];
}
