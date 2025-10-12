using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Commands.Build;
using SettlersOfCrutan.Application.Games.Commands.TurnFlow;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Presentation.Auth;
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
            IUserProvider userProvider,
            ICommandHandler<BuildInitialCommand> handler,
            CancellationToken ct) =>
        {
            PlayerId playerId = new() { Value = userProvider.GetUserId() };
            GameId gameId = new() { Value = id };
            Vertex settlementCoordinate = request.SettlementVertexCoordinate.ToDomain();
            Edge edgeCoordinate = request.RoadEdgeCoordinate.ToDomain();
            var cmd = new BuildInitialCommand(gameId, playerId, settlementCoordinate, edgeCoordinate);
            Result<Nothing> result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();

        });

        group.MapPost("/roll-dice", async Task<Results<Ok<RollDiceCommandResult>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromServices] IUserProvider userProvider,
            ICommandHandler<RollDiceCommand, RollDiceCommandResult> handler,
            CancellationToken ct) =>
        {
            PlayerId playerId = new() { Value = userProvider.GetUserId() };
            GameId gameId = new() { Value = id };
            var cmd = new RollDiceCommand(gameId, playerId);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });


        group.RequireAuthorization();

        return app;
    }
}