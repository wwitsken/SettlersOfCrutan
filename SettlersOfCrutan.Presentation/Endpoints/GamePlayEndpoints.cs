using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Games.Commands.Build;
using SettlersOfCrutan.Application.Games.Commands.Lifecycle;
using SettlersOfCrutan.Application.Games.Commands.TurnFlow;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Presentation.Dtos;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GamePlayEndpoints
{
    public static IEndpointRouteBuilder MapGamePlayEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games/{id:guid}/play").WithTags("Games", "Play");

        group.MapPost("/create", async Task<Results<Created, ValidationProblem>> (
            [FromBody] CreateGameRequest command,
            CreateGameCommandHandler handler,
            CancellationToken ct) =>
        {

            var cmd = new CreateGameCommand(command.GameName, [.. command.UserIds], GameType.BaseGame);
            var result = await handler.Handle(cmd, ct);
            return result.IsSuccess
                ? TypedResults.Created($"/games/{result.Value.Value}")
                : TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
        });

        group.MapPost("/{id:guid}/join", async Task<Results<Ok, NotFound, ValidationProblem>> (
            Guid id,
            [FromBody] JoinGameRequest request,
            JoinGameCommandHandler handler,
            CancellationToken ct) =>
        {
            PlayerId playerId = new() { Value = request.PlayerId };
            GameId gameId = new() { Value = id };
            var cmd = new JoinGameCommand(gameId, playerId);

            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess)
                return TypedResults.Ok();
            if (result.IsFailure && result.Error.Code == "Validation")
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    [result.Error.Code] = [result.Error.Message]
                });
            }
            return TypedResults.NotFound();
        });

        group.MapPost("/{id:guid}/place-initial", async Task<Results<Created, ValidationProblem, NotFound>> (
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
            if (result.IsSuccess)
                return TypedResults.Created();
            if (result.IsFailure && result.Error.Code == "Validation")
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    [result.Error.Code] = [result.Error.Message]
                });
            }
            return TypedResults.NotFound();
        });

        group.MapPost("/{id:guid}/roll-dice", async Task<Results<Ok<int>, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] RollDiceRequest request,
            RollDiceCommandHandler handler,
            CancellationToken ct) =>
        {
            PlayerId playerId = new() { Value = request.PlayerId };
            GameId gameId = new() { Value = id };

            var cmd = new RollDiceCommand(gameId, playerId);
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess)
                return TypedResults.Ok(result.Value.Item1 + result.Value.Item2);
            if (result.IsFailure && result.Error.Code == "Validation")
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    [result.Error.Code] = [result.Error.Message]
                });
            }
            return TypedResults.NotFound();
        });

        return app;
    }
}