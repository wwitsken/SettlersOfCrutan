using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Commands.Lifecycle;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Application.Games.Queries;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Presentation.Dtos;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class BaseGameEndpoints
{
    public static IEndpointRouteBuilder MapBaseGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games").WithTags("Game:Lifecycle");

        group.MapPost("/create", async Task<IResult> (
            [FromBody] CreateGameRequest command,
            ICommandHandler<CreateGameCommand, CreateGameResultDto> handler,
            CancellationToken ct) =>
                {
                    var cmd = new CreateGameCommand(command.GameName, [.. command.UserIds], command.GameType);
                    var result = await handler.Handle(cmd, ct);
                    return result.ToHttpResult(created: true, createdUri: $"/games/{result.Value.GameId}");
                });

        group.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            [FromServices] IQueryHandler<GetGameByIdQuery, Game> handler,
            CancellationToken ct) =>
        {
            var query = new GetGameByIdQuery(new GameId { Value = id });
            var result = await handler.Handle(query, ct);
            return result.ToHttpResult();
        });

        return app;
    }
};