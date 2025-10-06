using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Commands.Build;
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

        group.MapPost("/place-initial", async Task<IResult> (
            Guid id,
            [FromBody] BuildInitialRequest request,
            ICommandHandler<BuildInitialCommand> handler,
            CancellationToken ct) =>
        {
            PlayerId playerId = new() { Value = request.PlayerId };
            GameId gameId = new() { Value = id };
            Vertex settlementCoordinate = request.SettlementVertexCoordinate.ToDomain();
            Edge edgeCoordinate = request.RoadEdgeCoordinate.ToDomain();
            var cmd = new BuildInitialCommand(gameId, playerId, settlementCoordinate, edgeCoordinate);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();

        });

        group.MapPost("/roll-dice", async Task<IResult> (
            Guid id,
            [FromBody] RollDiceRequest request,
            ICommandHandler<RollDiceCommand, (int d1, int d2)> handler,
            CancellationToken ct) =>
        {
            PlayerId playerId = new() { Value = request.PlayerId };
            GameId gameId = new() { Value = id };
            var cmd = new RollDiceCommand(gameId, playerId);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();

        });

        return app;
    }
}