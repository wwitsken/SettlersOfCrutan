using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.DTOs;

// Disambiguate from similarly-named DTOs in other layers.
using AppBoardDto = SettlersOfCrutan.Application.Games.DTOs.BoardDto;
using AppHexDto = SettlersOfCrutan.Application.Games.DTOs.HexDto;
using AppHexCoordinateDto = SettlersOfCrutan.Application.Games.DTOs.HexCoordinateDto;
using AppPopulationCenterDto = SettlersOfCrutan.Application.Games.DTOs.PopulationCenterDto;
using AppRoadDto = SettlersOfCrutan.Application.Games.DTOs.RoadDto;
using AppPortDto = SettlersOfCrutan.Application.Games.DTOs.PortDto;

public static class GameMappingExtensions
{
    public static PublicGameDto ToDto(this Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        var buildingVp = GamePresentationScoring.BuildingVictoryPoints(game);
        var longestRoad = GamePresentationScoring.LongestRoadHolders(game);
        var largestArmy = GamePresentationScoring.LargestArmyHolders(game);

        return new PublicGameDto
        {
            Id = game.Id.Value,
            GameType = game.GameType.ToString(),
            GameName = game.GameName,
            Board = game.Board.ToDto(),
            BankResourceHand = game.BankResourceHand.Cards.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            BankDevCardHand = game.BankDevCardHand.Cards.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            TurnExpiresAt = game.TurnExpiresAt,
            PlayerDirection = game.PlayerDirection,
            GamePhase = game.GamePhase,
            Round = game.Round,
            CurrentPlayerId = game.Players[game.PlayerIndex].Id,
            CurrentTradeOffer = game.CurrentTradeOffer?.ToDto(),
            Players = [.. game.Players.Select((p, idx) => new PlayerDto {
                Id = p.Id.Value,
                PlayOrder = idx,
                IsPlaying = game.CurrentPlayerId() == p.Id,
                DisplayName = p.DisplayName,
                PlayerColor = p.Color,
                ResourceCardCount = p.TotalResources,
                DevelopmentCardCount = p.DevCardCount,
                PieceReserve = p.GetBuildables().ToDictionary(),
                DiscardRequirement = game.DiscardHalfRequirements.FirstOrDefault(r => r.PlayerId.Equals(p.Id))?.ResourceAmount ?? 0,
                VictoryPoints = buildingVp.GetValueOrDefault(p.Id, 0),
                HasLongestRoad = longestRoad.Contains(p.Id),
                HasLargestArmy = largestArmy.Contains(p.Id),
            })]
        };
    }

    public static PrivateGameDto ToPrivateDto(this Game game, string userId)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(userId);
        var player = game.Players.First(p => p.UserId == userId);
        return new PrivateGameDto
        {
            MyPlayerId = player.Id,
            MyHand = new MyHandDto(
                Resources: player.GetResources(),
                DevCards: player.GetDevelopmentCards(),
                Buildables: player.GetBuildables()
            ),
            MyScore = 1000, // TODO: Implement score calculation
            BuildableRoads = [.. game.GetBuildableRoads(player.Id).Select(ToHexCoordinateDtos)],
            BuildableSettlements = [.. game.GetBuildableSettlements(player.Id).Select(ToHexCoordinateDtos)]
        };
    }

    private static AppBoardDto ToDto(this Board board)
    {
        ArgumentNullException.ThrowIfNull(board);

        return new AppBoardDto
        {
            Hexes = [.. board.Hexes.Select(h => new AppHexDto
            {
                Coordinate = new AppHexCoordinateDto { Q = h.Coordinate.Q, R = h.Coordinate.R },
                Resource = h.Resource.ToString(),
                NumberToken = h.NumberToken ?? 0,
                HasRobber = h.HasRobber
            })],
            PopulationCenters = [.. board.PopulationCenters.Select(pc => new AppPopulationCenterDto
            {
                Coordinates = ToHexCoordinateDtos(pc.VertexCoordinate),
                Type = pc.Level.ToString(),
                PlayerOwnerId = pc.PlayerOwner.Value
            })],
            Roads = [.. board.Roads.Select(r => new AppRoadDto
            {
                Coordinates = ToHexCoordinateDtos(r.EdgeCoordinate),
                PlayerOwnerId = r.OwnerId.Value
            })],
            Ports = [.. board.Ports.Select(p =>
            {
                var (inCoord, outCoord) = GetPortInOutCoordinates(board, p.EdgeCoordinate);

                return new AppPortDto
                {
                    InCoordinate = inCoord,
                    OutCoordinate = outCoord,
                    Coordinates = ToHexCoordinateDtos(p.EdgeCoordinate),
                    Type = p.Type.ToString()
                };
            })]
        };
    }

    private static (AppHexCoordinateDto In, AppHexCoordinateDto Out) GetPortInOutCoordinates(Board board, Edge edge)
    {
        ArgumentNullException.ThrowIfNull(board);

        var coords = edge.HexCoords().ToArray();

        static AppHexCoordinateDto ToDto(HexCoord hc) => new() { Q = hc.Q, R = hc.R };

        static bool IsLand(Board b, HexCoord hc)
            => b.Hexes.Any(h => h.Coordinate == hc && h.Resource != ResourceCardType.Water);

        static bool IsWaterOrOffBoard(Board b, HexCoord hc)
            => b.Hexes.Any(h => h.Coordinate == hc && h.Resource == ResourceCardType.Water)
                || !b.Hexes.Any(h => h.Coordinate == hc);

        var inHex = coords.First(hc => IsLand(board, hc));
        var outHex = coords.First(hc => IsWaterOrOffBoard(board, hc));

        return (ToDto(inHex), ToDto(outHex));
    }

    private static List<AppHexCoordinateDto> ToHexCoordinateDtos(Vertex v)
        =>
        [
            new AppHexCoordinateDto { Q = v.HexCoord1.Q, R = v.HexCoord1.R },
            new AppHexCoordinateDto { Q = v.HexCoord2.Q, R = v.HexCoord2.R },
            new AppHexCoordinateDto { Q = v.HexCoord3.Q, R = v.HexCoord3.R }
        ];

    private static List<AppHexCoordinateDto> ToHexCoordinateDtos(Edge e)
        =>
        [
            new AppHexCoordinateDto { Q = e.HexCoord1.Q, R = e.HexCoord1.R },
            new AppHexCoordinateDto { Q = e.HexCoord2.Q, R = e.HexCoord2.R }
        ];

    private static TradeOfferDto ToDto(this TradeOffer offer)
    {
        ArgumentNullException.ThrowIfNull(offer);

        return new TradeOfferDto
        {
            PlayerProposerId = offer.ProposerId.Value,
            PlayerAcceptorId = offer.AcceptorId?.Value,
            RequestedResources = offer.RequestedResources.ToDictionary(r => r.Type, r => r.Quantity),
            OfferedResources = offer.OfferedResources.ToDictionary(r => r.Type, r => r.Quantity),
            IsAccepted = offer.IsAccepted
        };
    }
}
