using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Users;

namespace SettlersOfCrutan.Infrastructure.Auth;

public sealed class HttpContextCurrentUser(
    IHttpContextAccessor httpContextAccessor,
    IUserRepository userRepository) : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<UserId> UserId()
    {
        var http = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HttpContext.");

        if (http.User?.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException("User is not authenticated.");

        Claim? id = http.User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? http.User.FindFirst("sub");
        var principalId = id?.Value ?? throw new InvalidOperationException("User id claim not found.");

        var existing = await _userRepository.GetByPrincipalIdAsync(principalId);
        if (existing is not null)
            return existing.Id;

        var created = User.RegisterNew(principalId);
        var saved = await _userRepository.SaveAsync(created);
        if (!saved)
            throw new InvalidOperationException("Failed to persist new user.");

        return created.Id;
    }
}
