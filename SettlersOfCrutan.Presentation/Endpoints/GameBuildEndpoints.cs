using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Games.Commands.Build;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Presentation.Dtos;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GameBuildEndpoints
{
    public static IEndpointRouteBuilder MapGameBuildEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games/{id:guid}/build").WithTags("Games:Build");

        group.MapPost("/road", async Task<Results<Created, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] BuildRoadRequest request,
            BuildRoadCommandHandler handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            Edge edge = request.EdgeCoordinate.ToDomain();

            var cmd = new BuildRoadCommand(gameId, playerId, edge);
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess) return TypedResults.Created($"/games/{id}");
            if (result.IsFailure && result.Error.Code == "Validation")
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
            return TypedResults.NotFound();
        });

        group.MapPost("/settlement", async Task<Results<Created, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] BuildSettlementRequest request,
            BuildSettlementCommandHandler handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            Vertex vertex = request.VertexCoordinate.ToDomain();

            var cmd = new BuildSettlementCommand(gameId, playerId, vertex);
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess) return TypedResults.Created($"/games/{id}");
            if (result.IsFailure && result.Error.Code == "Validation")
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
            return TypedResults.NotFound();
        });

        group.MapPost("/city", async Task<Results<Created, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] UpgradeSettlementToCityRequest request,
            UpgradeSettlementToCityCommandHandler handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            Vertex vertex = request.VertexCoordinate.ToDomain();

            var cmd = new UpgradeSettlementToCityCommand(gameId, playerId, vertex);
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess) return TypedResults.Created($"/games/{id}");
            if (result.IsFailure && result.Error.Code == "Validation")
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
            return TypedResults.NotFound();
        });

        group.MapPost("/development-card", async Task<Results<Created, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] BuyDevelopmentCardRequest request,
            BuyDevelopmentCardCommandHandler handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };

            var cmd = new BuyDevelopmentCardCommand(gameId, playerId);
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess) return TypedResults.Created($"/games/{id}");
            if (result.IsFailure && result.Error.Code == "Validation")
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
            return TypedResults.NotFound();
        });

        return app;
    }
}
