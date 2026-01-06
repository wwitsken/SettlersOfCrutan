namespace SettlersOfCrutan.Application.Games.DTOs;

public record BoardDto
{
    public List<HexDto> Hexes { get; set; }
    public List<PopulationCenterDto> PopulationCenters { get; set; }
    public List<RoadDto> Roads { get; set; }
    public List<PortDto> Ports { get; set; }
}

public record HexCoordinateDto
{
    public int Q { get; set; }
    public int R { get; set; }
}

public record HexDto
{
    public HexCoordinateDto Coordinate { get; set; }
    public string Resource { get; set; }
    public int NumberToken { get; set; }
    public bool HasRobber { get; set; }
}

public record PopulationCenterDto
{
    public List<HexCoordinateDto> Coordinates { get; set; }
    public string Type { get; set; } // e.g., "Settlement" or "City"
    public string PlayerOwnerId { get; set; }
}

public record RoadDto
{
    public List<HexCoordinateDto> Coordinates { get; set; }
    public string PlayerOwnerId { get; set; }
}

public record PortDto
{
    public List<HexCoordinateDto> Coordinates { get; set; }
    public string Type { get; set; } // e.g., "Wood", "Brick", or "Any"
}