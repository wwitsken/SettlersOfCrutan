using System.Security.Claims;

namespace SettlersOfCrutan.Presentation.Auth;

public sealed class UserProvider(IHttpContextAccessor httpContextAccessor) : IUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string GetUserId()
    {
        var http = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HttpContext.");

        if (http.User?.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException("User is not authenticated.");

        // Identity uses NameIdentifier by default; JWTs often use "sub".
        Claim? id = http.User.FindFirst(ClaimTypes.NameIdentifier)
                 ?? http.User.FindFirst("sub");

        return id?.ToString() ?? throw new InvalidOperationException("User id claim not found.");
    }
}
