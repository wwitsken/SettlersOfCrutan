using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Application.Users.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace SettlersOfCrutan.Application.Games.Queries;

public record GetUsersInGameQuery(GameId Id) : IQuery<PrincipalIdsInGameDto>;

public class GetUsersInGameQueryHandler(IGameRepository gameRepository,
                                        IUserRepository userRepository) : IQueryHandler<GetUsersInGameQuery, PrincipalIdsInGameDto>
{
    async Task<Result<PrincipalIdsInGameDto>> IQueryHandler<GetUsersInGameQuery, PrincipalIdsInGameDto>.Handle(GetUsersInGameQuery query, CancellationToken ct)
    {
        Game? game = await gameRepository.GetAsync(query.Id, ct);
        if (game is null) return Result<PrincipalIdsInGameDto>.Failure(DomainError.NotFound);

        IReadOnlyList<User> users = await userRepository.GetManyAsync(game.Players.Select(p => p.UserId), ct);
        return Result<PrincipalIdsInGameDto>.Success(new([.. users.Select(u => u.PrincipalId)]));
    }
}