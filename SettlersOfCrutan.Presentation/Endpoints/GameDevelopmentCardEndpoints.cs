using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Games.Commands.DevelopmentCards;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Presentation.Dtos;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GameDevelopmentCardEndpoints
{
    public static IEndpointRouteBuilder MapGameDevelopmentCardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games/{id:guid}/devcards").WithTags("Games:DevCards");

        group.MapPost("/road-building", async Task<Results<Ok, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] UseRoadBuildingRequest request,
            UseRoadBuildingCommandHandler handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            var cmd = new UseRoadBuildingCommand(gameId, playerId, request.Edge1.ToDomain(), request.Edge2.ToDomain());
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess) return TypedResults.Ok();
            if (result.IsFailure && result.Error.Code == "Validation")
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
            return TypedResults.NotFound();
        });

        group.MapPost("/monopoly", async Task<Results<Ok<int>, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] UseMonopolyRequest request,
            UseMonopolyCommandHandler handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            var cmd = new UseMonopolyCommand(gameId, playerId, request.ResourceType);
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess) return TypedResults.Ok(result.Value);
            if (result.IsFailure && result.Error.Code == "Validation")
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
            return TypedResults.NotFound();
        });

        group.MapPost("/year-of-plenty", async Task<Results<Ok<(ResourceCardType, ResourceCardType)>, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] UseYearOfPlentyRequest request,
            UseYearOfPlentyCommandHandler handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            var cmd = new UseYearOfPlentyCommand(gameId, playerId, request.Resource1, request.Resource2);
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess) return TypedResults.Ok(result.Value);
            if (result.IsFailure && result.Error.Code == "Validation")
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
            return TypedResults.NotFound();
        });

        group.MapPost("/knight", async Task<Results<Ok<HexCoord>, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] UseKnightRequest request,
            UseKnightCommandHandler handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            var cmd = new UseKnightCommand(gameId, playerId, request.NewRobberHex.ToDomain(), new PlayerId { Value = request.VictimPlayerId });
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess) return TypedResults.Ok(result.Value);
            if (result.IsFailure && result.Error.Code == "Validation")
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
            return TypedResults.NotFound();
        });

        return app;
    }
}
