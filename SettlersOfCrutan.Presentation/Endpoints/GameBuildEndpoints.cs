using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Commands.Build;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Presentation.Dtos;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GameBuildEndpoints
{
    public static IEndpointRouteBuilder MapGameBuildEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games/{id:guid}/build").WithTags("Game:Build");

        group.MapPost("/road", async Task<IResult> (
            Guid id,
            [FromBody] BuildRoadRequest request,
            ICommandHandler<BuildRoadCommand, Road> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            Edge edge = request.EdgeCoordinate.ToDomain();
            var cmd = new BuildRoadCommand(gameId, playerId, edge);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult(true, $"/games/{id}");
        });

        group.MapPost("/settlement", async Task<IResult> (
            Guid id,
            [FromBody] BuildSettlementRequest request,
            ICommandHandler<BuildSettlementCommand, PopulationCenter> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            Vertex vertex = request.VertexCoordinate.ToDomain();
            var cmd = new BuildSettlementCommand(gameId, playerId, vertex);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult(true, $"/games/{id}");

        });

        group.MapPost("/city", async Task<IResult> (
            Guid id,
            [FromBody] UpgradeSettlementToCityRequest request,
            ICommandHandler<UpgradeSettlementToCityCommand, PopulationCenter> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            Vertex vertex = request.VertexCoordinate.ToDomain();
            var cmd = new UpgradeSettlementToCityCommand(gameId, playerId, vertex);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult(true, $"/games/{id}");

        });

        group.MapPost("/development-card", async Task<IResult> (
            Guid id,
            [FromBody] BuyDevelopmentCardRequest request,
            ICommandHandler<BuyDevelopmentCardCommand, DevelopmentCardType> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            var cmd = new BuyDevelopmentCardCommand(gameId, playerId);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult(true, $"/games/{id}");

        });

        return app;
    }
}
