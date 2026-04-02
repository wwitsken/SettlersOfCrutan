using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace SettlersOfCrutan.Presentation.Auth;

/// <summary>
/// Development-only scheme: authenticates from <see cref="DevUserImpersonation.HeaderName"/> or hub query
/// <see cref="DevUserImpersonation.SignalRQueryParameter"/>.
/// </summary>
public sealed class DevUserIdAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IHostEnvironment environment)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "DevUserId";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!environment.IsDevelopment())
            return Task.FromResult(AuthenticateResult.NoResult());

        string? userId = null;
        if (Request.Headers.TryGetValue(DevUserImpersonation.HeaderName, out var headerValues))
            userId = headerValues.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(userId)
            && Request.Path.StartsWithSegments("/api/realtime-hub")
            && Request.Query.TryGetValue(DevUserImpersonation.SignalRQueryParameter, out var queryValues))
            userId = queryValues.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(userId))
            return Task.FromResult(AuthenticateResult.Fail("Dev user id missing or empty."));

        userId = userId.Trim();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("sub", userId),
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
