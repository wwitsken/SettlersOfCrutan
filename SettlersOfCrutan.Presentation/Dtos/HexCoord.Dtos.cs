using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Presentation.Dtos;

public record HexCoordDto(int Q, int R, int S)
{
    public static HexCoordDto FromDomain(HexCoord coord) => new(coord.Q, coord.R, coord.S);
    public HexCoord ToDomain() => new(Q, R, S);
};

public record VertexCoordDto(HexCoordDto HexCoord1, HexCoordDto HexCoord2, HexCoordDto HexCoord3)
{
    public static VertexCoordDto FromDomain(Vertex coord)
        => new(HexCoordDto.FromDomain(coord.HexCoord1), HexCoordDto.FromDomain(coord.HexCoord2), HexCoordDto.FromDomain(coord.HexCoord3));
    public Vertex ToDomain() => new(HexCoord1.ToDomain(), HexCoord2.ToDomain(), HexCoord3.ToDomain());
};

public record EdgeCoordDto(HexCoordDto HexCoord1, HexCoordDto HexCoord2)
{
    public static EdgeCoordDto FromDomain(Edge coord)
        => new(HexCoordDto.FromDomain(coord.HexCoord1), HexCoordDto.FromDomain(coord.HexCoord2));
    public Edge ToDomain() => new(HexCoord1.ToDomain(), HexCoord2.ToDomain());
};