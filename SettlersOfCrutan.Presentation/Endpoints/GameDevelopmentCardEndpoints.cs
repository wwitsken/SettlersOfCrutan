using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Commands.DevelopmentCards;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Presentation.Dtos;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GameDevelopmentCardEndpoints
{
    public static IEndpointRouteBuilder MapGameDevelopmentCardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games/{id:guid}/devcards").WithTags("Game:DevCards");

        group.MapPost("/road-building", async Task<IResult> (

            Guid id,
            [FromBody] UseRoadBuildingRequest request,
            ICommandHandler<UseRoadBuildingCommand, (Road r1, Road r2)> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            var cmd = new UseRoadBuildingCommand(gameId, playerId, request.Edge1.ToDomain(), request.Edge2.ToDomain());
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/monopoly", async Task<IResult> (
            Guid id,
            [FromBody] UseMonopolyRequest request,
            ICommandHandler<UseMonopolyCommand, int> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            var cmd = new UseMonopolyCommand(gameId, playerId, request.ResourceType);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();

        });

        group.MapPost("/year-of-plenty", async Task<IResult> (
            Guid id,
            [FromBody] UseYearOfPlentyRequest request,
            ICommandHandler<UseYearOfPlentyCommand, (ResourceCardType t1, ResourceCardType t2)> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            var cmd = new UseYearOfPlentyCommand(gameId, playerId, request.Resource1, request.Resource2);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();

        });

        group.MapPost("/knight", async Task<IResult> (
            Guid id,
            [FromBody] UseKnightRequest request,
            ICommandHandler<UseKnightCommand, HexCoord> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            var cmd = new UseKnightCommand(gameId, playerId, request.NewRobberHex.ToDomain(), new PlayerId { Value = request.VictimPlayerId });
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();

        });

        return app;
    }
}
