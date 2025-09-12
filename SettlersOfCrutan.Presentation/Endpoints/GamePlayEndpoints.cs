using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Games.Commands.Build;
using SettlersOfCrutan.Application.Games.Commands.Lifecycle;
using SettlersOfCrutan.Application.Games.Commands.TurnFlow;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Presentation.Dtos;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GamePlayEndpoints
{
    public static IEndpointRouteBuilder MapGamePlayEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games/{id:guid}/play").WithTags("Game:Play");

        group.MapPost("/join", async Task<IResult> (
            Guid id,
            [FromBody] JoinGameRequest request,
            JoinGameCommandHandler handler,
            CancellationToken ct) =>
        {
            PlayerId playerId = new() { Value = request.PlayerId };
            GameId gameId = new() { Value = id };
            var cmd = new JoinGameCommand(gameId, playerId);
            var result = await handler.Handle(cmd, ct);
            return result.IsSuccess ? TypedResults.Ok() : result.Error.ToHttpResult();
        });

        group.MapPost("/place-initial", async Task<IResult> (
            Guid id,
            [FromBody] BuildInitialRequest request,
            BuildInitialCommandHandler handler,
            CancellationToken ct) =>
        {
            PlayerId playerId = new() { Value = request.PlayerId };
            GameId gameId = new() { Value = id };
            Vertex settlementCoordinate = request.SettlementVertexCoordinate.ToDomain();
            Edge edgeCoordinate = request.RoadEdgeCoordinate.ToDomain();
            var cmd = new BuildInitialCommand(gameId, playerId, settlementCoordinate, edgeCoordinate);
            var result = await handler.Handle(cmd, ct);
            return result.IsSuccess ? TypedResults.Created() : result.Error.ToHttpResult();
        });

        group.MapPost("/roll-dice", async Task<IResult> (
            Guid id,
            [FromBody] RollDiceRequest request,
            RollDiceCommandHandler handler,
            CancellationToken ct) =>
        {
            PlayerId playerId = new() { Value = request.PlayerId };
            GameId gameId = new() { Value = id };
            var cmd = new RollDiceCommand(gameId, playerId);
            var result = await handler.Handle(cmd, ct);
            return result.IsSuccess ? TypedResults.Ok(result.Value.Item1 + result.Value.Item2) : result.Error.ToHttpResult();
        });

        return app;
    }
}