using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SettlersOfCrutan.Presentation;

internal sealed class EntraExternalIdOAuth2DocumentTransformer(IConfiguration configuration)
    : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authority =
            configuration["AUTH_AUTHORITY"]
            ?? Environment.GetEnvironmentVariable("AUTH_AUTHORITY")
            ?? throw new InvalidOperationException("AUTH_AUTHORITY is not set.");

        var audience =
            configuration["AUTH_AUDIENCE"]
            ?? Environment.GetEnvironmentVariable("AUTH_AUDIENCE")
            ?? throw new InvalidOperationException("AUTH_AUDIENCE is not set.");

        var baseAuthority = authority.TrimEnd('/');

        if (baseAuthority.EndsWith("/v2.0", StringComparison.OrdinalIgnoreCase))
        {
            baseAuthority = baseAuthority[..^"/v2.0".Length];
        }

        var authorizationUrl = new Uri($"{baseAuthority}/oauth2/v2.0/authorize");
        var tokenUrl = new Uri($"{baseAuthority}/oauth2/v2.0/token");

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["OAuth2"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = authorizationUrl,
                    TokenUrl = tokenUrl,
                    Scopes = new Dictionary<string, string>
                    {
                        { $"{audience}/access_as_user", "Access the SettlersOfCrutan API as the signed-in user" }
                    }
                }
            }
        };

        document.Security = [
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("OAuth2"),
                    [$"{audience}/access_as_user"]
                }
            }
        ];

        document.SetReferenceHostDocument();

        return Task.CompletedTask;

    }
}
