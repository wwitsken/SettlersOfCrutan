using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Games.Commands.TurnFlow;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Presentation.Dtos;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GameTurnFlowEndpoints
{
    public static IEndpointRouteBuilder MapGameTurnFlowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games/{id:guid}/turn").WithTags("Games:TurnFlow");

        group.MapPost("/end", async Task<Results<Ok<PlayerId>, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] EndTurnRequest request,
            EndTurnCommandHandler handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            var cmd = new EndTurnCommand(gameId, playerId);
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess) return TypedResults.Ok(result.Value);
            if (result.IsFailure && result.Error.Code == "Validation")
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
            return TypedResults.NotFound();
        });

        group.MapPost("/robber/resolve", async Task<Results<Ok<ResourceCardType>, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] ResolveRobberRequest request,
            ResolveRobberCommandHandler handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            PlayerId victimId = new() { Value = request.VictimPlayerId };
            var cmd = new ResolveRobberCommand(gameId, playerId, request.NewRobberHex.ToDomain(), victimId);
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess) return TypedResults.Ok(result.Value);
            if (result.IsFailure && result.Error.Code == "Validation")
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
            return TypedResults.NotFound();
        });

        group.MapPost("/discard-half", async Task<Results<Ok, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] DiscardHalfRequest request,
            DiscardHalfCommandHandler handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            var cmd = new DiscardHalfCommand(gameId, playerId, request.Discards.Select(d => new ResourceCardAmount(d.Type, d.Quantity)).ToList());
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess) return TypedResults.Ok();
            if (result.IsFailure && result.Error.Code == "Validation")
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
            return TypedResults.NotFound();
        });

        return app;
    }
}
