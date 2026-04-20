using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Users.Commands;
using SettlersOfCrutan.Application.Users.DTOs;
using SettlersOfCrutan.Application.Users.Queries;
using SettlersOfCrutan.Domain.Users;
using SettlersOfCrutan.Presentation.Dtos;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class UserProfileEndpoints
{
    public static IEndpointRouteBuilder MapUserProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        group.MapGet("/", async Task<Results<Ok<IEnumerable<UserProfileDto>>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            [FromBody] GetUserProfilesRequest request,
            [FromServices] IQueryHandler<GetUserProfilesByIds, IEnumerable<UserProfileDto>> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetUserProfilesByIds([.. request.UserIds.Select(u => new UserId() { Value = u })]), ct);
            return result.ToHttpResult();
        });

        group.MapGet("/{userId:guid}", async Task<Results<Ok<UserProfileDto>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid userId,
            [FromServices] IQueryHandler<GetUserProfileById, UserProfileDto> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetUserProfileById(new() { Value = userId }), ct);
            return result.ToHttpResult();
        });

        group.MapGet("/me", async Task<Results<Ok<UserProfileDto>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            [FromServices] IQueryHandler<GetCurrentUserProfile, UserProfileDto> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetCurrentUserProfile(), ct);
            return result.ToHttpResult();
        });

        group.MapPut("/me", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            [FromBody] UpdateUserProfileRequest body,
            [FromServices] ICommandHandler<UpdateUserProfileCommand> handler,
            CancellationToken ct) =>
        {
            var cmd = new UpdateUserProfileCommand(body.DisplayName, body.PreferredColor);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.RequireAuthorization();

        return app;
    }
}
