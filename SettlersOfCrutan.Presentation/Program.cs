using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Identity.Web;
using Scalar.AspNetCore;
using SettlersOfCrutan.Application;
using SettlersOfCrutan.Infrastructure;
using SettlersOfCrutan.Infrastructure.Redis;
using SettlersOfCrutan.Infrastructure.Redis.Serialization;
using SettlersOfCrutan.Infrastructure.SignalR;
using SettlersOfCrutan.Presentation;
using SettlersOfCrutan.Presentation.Auth;
using SettlersOfCrutan.Presentation.Endpoints;
using SettlersOfCrutan.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddRedisClient("redis");

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<EntraExternalIdOAuth2DocumentTransformer>();
});
builder.Services.AddApplicationServices();
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
builder.Services.AddInfrastructureServices();
builder.Services.AddSignalR()
    .AddJsonProtocol(o => o.PayloadSerializerOptions = ApiSharedJsonOptions.CreateForSignalR())
    .AddStackExchangeRedis(builder.Configuration.GetConnectionString("redis")!);

// HTTP Context
builder.Services.AddHttpContextAccessor();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IUserProvider, DevelopmentUserProvider>();
    const string devOrJwtScheme = "DevOrJwt";
    builder.Services.AddAuthentication(devOrJwtScheme)
        .AddPolicyScheme(devOrJwtScheme, devOrJwtScheme, options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                if (context.Request.Headers.TryGetValue(DevUserImpersonation.HeaderName, out var headerValues))
                {
                    var h = headerValues.ToString();
                    if (!string.IsNullOrWhiteSpace(h))
                        return DevUserIdAuthenticationHandler.SchemeName;
                }

                if (context.Request.Path.StartsWithSegments("/api/realtime-hub")
                    && context.Request.Query.TryGetValue(DevUserImpersonation.SignalRQueryParameter, out var q)
                    && !string.IsNullOrWhiteSpace(q))
                    return DevUserIdAuthenticationHandler.SchemeName;

                return JwtBearerDefaults.AuthenticationScheme;
            };
        })
        .AddScheme<AuthenticationSchemeOptions, DevUserIdAuthenticationHandler>(
            DevUserIdAuthenticationHandler.SchemeName,
            displayName: null,
            configureOptions: null)
        .AddMicrosoftIdentityWebApi(builder.Configuration);
}
else
{
    builder.Services.AddScoped<IUserProvider, UserProvider>();
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration);
}


builder.Services.AddAuthorizationBuilder()
  .AddPolicy("AccessAsUser", policy =>
    {
        policy.RequireClaim("scp", "access_as_user");
    });

// Flatten BaseId value objects in HTTP JSON (responses and requests)
builder.Services.AddHttpJsonSettings();

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment())
{
    var audience =
    builder.Configuration["AzureAd:Instance"]
    ?? Environment.GetEnvironmentVariable("AzureAd:Instance")
    ?? throw new InvalidOperationException("AzureAd:Instance is not set.");

    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .AddPreferredSecuritySchemes("OAuth2")
        .AddAuthorizationCodeFlow("OAuth2", flow =>
        {
            flow.ClientId = "6d3e2927-22db-4dde-8446-92585e8b3ae7";
            flow.Pkce = Pkce.Sha256;
            flow.SelectedScopes = [$"{audience}/access_as_user"];
        }));
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("api/")
    .MapBaseGameEndpoints()
    .MapGamePlayEndpoints()
    .MapGameBuildEndpoints()
    .MapGameTradeEndpoints()
    .MapGameDevelopmentCardEndpoints()
    .MapGameTurnFlowEndpoints()
    .MapLobbyEndpoints();

app.MapGet("/api/health", Results<Ok<string>, BadRequest> () => TypedResults.Ok($"OK at {DateTime.Now:t}"));
app.MapGet("/api/test", Results<Ok<string>, BadRequest> () => TypedResults.Ok("Test endpoint is working")).RequireAuthorization();

app.MapHub<CrutanHub>("/api/realtime-hub").RequireAuthorization();

app.Run();