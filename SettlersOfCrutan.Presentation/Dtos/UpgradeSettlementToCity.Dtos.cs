using SettlersOfCrutan.Application.Games.DTOs;

namespace SettlersOfCrutan.Presentation.Dtos;

public record UpgradeSettlementToCityRequest(string PlayerId, VertexDto VertexCoordinate);
