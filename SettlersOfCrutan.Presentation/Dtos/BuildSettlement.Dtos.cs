using SettlersOfCrutan.Application.Games.DTOs;

namespace SettlersOfCrutan.Presentation.Dtos;

public record BuildSettlementRequest(string PlayerId, VertexDto VertexCoordinate);
