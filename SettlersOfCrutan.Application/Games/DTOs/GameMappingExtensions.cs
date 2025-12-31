using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using System.Runtime.CompilerServices;

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
    public static GameDto ToDto(this Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        return new GameDto
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
            PlayerIndex = game.PlayerIndex,
            CurrentTradeOffer = game.CurrentTradeOffer?.ToDto(),
            Players = [.. game.Players.Select((p, idx) => p.ToPublicDto(idx, idx == game.PlayerIndex))]
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
                OwnerId = pc.PlayerOwner.Value
            })],
            Roads = [.. board.Roads.Select(r => new AppRoadDto
            {
                Coordinates = ToHexCoordinateDtos(r.EdgeCoordinate),
                OwnerId = r.OwnerId.Value
            })],
            Ports = [.. board.Ports.Select(p => new AppPortDto
            {
                Coordinates = ToHexCoordinateDtos(p.EdgeCoordinate),
                Type = p.Type.ToString()
            })]
        };
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
            ProposerId = offer.ProposerId.Value,
            AcceptorId = offer.AcceptorId?.Value,
            RequestedResources = offer.RequestedResources.ToDictionary(r => r.Type, r => r.Quantity),
            OfferedResources = offer.OfferedResources.ToDictionary(r => r.Type, r => r.Quantity),
            IsAccepted = offer.IsAccepted
        };
    }

    private static PublicPlayerDto ToPublicDto(this Player player, int playerOrder, bool isPlaying)
    {
        ArgumentNullException.ThrowIfNull(player);

        return new PublicPlayerDto
        {
            Id = player.Id.Value,
            PlayOrder = playerOrder,
            IsPlaying = isPlaying,
            DisplayName = player.DisplayName,
            PlayerColor = player.Color,
            ResourceCardCount = player.TotalResources,
            DevelopmentCardCount = player.DevCardCount,
            PieceReserve = player.GetBuildables().ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            DiscardRequirement = 0
        };
    }
}
