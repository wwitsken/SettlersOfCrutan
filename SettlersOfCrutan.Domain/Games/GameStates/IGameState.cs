namespace SettlersOfCrutan.Domain.Games.GameStates;
/*
public interface IGameState
{
    Result<Decision> StartGame(Game game);
    Result<Decision<(int d1, int d2)>> RollAndResolveProduction(Game game, PlayerId playerId);
    Result<Decision> ResolveProduction(Game game, PlayerId playerId);
    Result<Decision> DiscardHalf(Game game, PlayerId playerId, List<ResourceAmount> discards);
    Result<Decision<RobberResult>> ResolveRobber(Game game, PlayerId playerId, HexCoord newRobberHexCoord, PlayerId victimId);
    Result<Decision<PlayerId>> EndTurn(Game game, PlayerId playerId);

    // Build/Purchase
    Result<Decision<(PopulationCenter pc, Road road)>> PlaceInitialAndEndTurn(Game game, PlayerId playerId, Vertex settlementVertex, Edge roadEdge);
    Result<Decision<Road>> BuildRoad(Game game, PlayerId playerId, Edge edge);
    Result<Decision<PopulationCenter>> BuildSettlement(Game game, PlayerId playerId, Vertex vertex);
    Result<Decision<PopulationCenter>> BuildCity(Game game, PlayerId playerId, Vertex vertex);
    Result<Decision<DevelopmentCardType>> BuyDevelopmentCard(Game game, PlayerId playerId);

    // Trade
    Result<Decision> MakeMaritimeTrade(Game game, PlayerId playerId, ResourceType requestResource, ResourceType discardResource, int discardResourceAmount);
    Result<Decision> ProposeTrade(Game game, PlayerId playerId, List<ResourceAmount> requested, List<ResourceAmount> offered);
    Result<Decision> AcceptTrade(Game game, PlayerId playerId, TradeOfferId tradeOfferId);

    // Dev Cards
    Result<Decision<(Road r1, Road r2)>> PlayRoadBuilding(Game game, PlayerId playerId, Edge edge1, Edge edge2);
    Result<Decision<int>> PlayMonopoly(Game game, PlayerId playerId, ResourceType resourceType);
    Result<Decision<(ResourceType t1, ResourceType t2)>> PlayYearOfPlenty(Game game, PlayerId playerId, ResourceType t1, ResourceType t2);
    Result<Decision<HexCoord>> PlayKnight(Game game, PlayerId playerId, HexCoord newRobberHexCoord, PlayerId victimId);
}
*/