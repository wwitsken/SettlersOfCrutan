using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Commands.Build;
using SettlersOfCrutan.Application.Games.Commands.TurnFlow;
using SettlersOfCrutan.Domain.Core;
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

        group.MapPost("/place-initial", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromBody] BuildInitialRequest request,
            ICommandHandler<BuildInitialCommand> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            Vertex settlementCoordinate = request.SettlementVertexCoordinate.ToDomain();
            Edge edgeCoordinate = request.RoadEdgeCoordinate.ToDomain();
            var cmd = new BuildInitialCommand(gameId, settlementCoordinate, edgeCoordinate);
            Result<Nothing> result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/roll-dice", async Task<Results<Ok<RollDiceCommandResult>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            ICommandHandler<RollDiceCommand, RollDiceCommandResult> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            var cmd = new RollDiceCommand(gameId);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.RequireAuthorization();

        return app;
    }
}
