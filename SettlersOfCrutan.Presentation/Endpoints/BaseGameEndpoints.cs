using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Commands.Lifecycle;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Application.Games.Queries;
using SettlersOfCrutan.Presentation.Auth;
using SettlersOfCrutan.Presentation.Dtos;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class BaseGameEndpoints
{
    public static IEndpointRouteBuilder MapBaseGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games").WithTags("Game:Lifecycle");

        group.MapPost("/create", async Task<Results<Ok<CreateGameResultDto>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            [FromBody] CreateGameRequest command,
            ICommandHandler<CreateGameCommand, CreateGameResultDto> handler,
            CancellationToken ct) =>
                {
                    var cmd = new CreateGameCommand(command.GameName, [.. command.UserIds], command.GameType);
                    var result = await handler.Handle(cmd, ct);
                    return result.ToHttpResult();
                });

        group.MapPost("/{id:guid}/join", async Task<Results<Ok<Guid>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            IUserProvider userProvider,
            ICommandHandler<JoinGameCommand, Guid> handler,
            CancellationToken ct) =>
        {
            var cmd = new JoinGameCommand(id, userProvider.GetUserId());
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapGet("/{id:guid}", async Task<Results<Ok<PlayerGameProjectionDto>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            IUserProvider userProvider,
            [FromServices] IQueryHandler<GetGameByIdQuery, PlayerGameProjectionDto> handler,
            CancellationToken ct) =>
        {
            var query = new GetGameByIdQuery(id, userProvider.GetUserId());
            var result = await handler.Handle(query, ct);
            return result.ToHttpResult();
        });

        return app;
    }
};