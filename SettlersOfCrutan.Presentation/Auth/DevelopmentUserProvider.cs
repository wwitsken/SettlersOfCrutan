using System.Security.Claims;

namespace SettlersOfCrutan.Presentation.Auth;

/// <summary>
/// Development: prefers <see cref="DevUserImpersonation"/> header, then hub query, then JWT claims.
/// </summary>
public sealed class DevelopmentUserProvider(IHttpContextAccessor httpContextAccessor) : IUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string GetUserId()
    {
        var http = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HttpContext.");

        if (http.Request.Headers.TryGetValue(DevUserImpersonation.HeaderName, out var headerValues))
        {
            var h = headerValues.ToString();
            if (!string.IsNullOrWhiteSpace(h))
                return h.Trim();
        }

        if (http.Request.Path.StartsWithSegments("/api/realtime-hub")
            && http.Request.Query.TryGetValue(DevUserImpersonation.SignalRQueryParameter, out var queryValues))
        {
            var q = queryValues.ToString();
            if (!string.IsNullOrWhiteSpace(q))
                return q.Trim();
        }

        if (http.User?.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException("User is not authenticated.");

        Claim? id = http.User.FindFirst(ClaimTypes.NameIdentifier)
                 ?? http.User.FindFirst("sub");

        return id?.Value ?? throw new InvalidOperationException("User id claim not found.");
    }
}
