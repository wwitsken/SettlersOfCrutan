namespace SettlersOfCrutan.Presentation.Dtos;

public record BuildInitialRequest(string PlayerId, VertexCoordDto SettlementVertexCoordinate, EdgeCoordDto RoadEdgeCoordinate);
