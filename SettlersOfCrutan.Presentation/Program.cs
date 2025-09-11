using Scalar.AspNetCore;
using SettlersOfCrutan.Application;
using SettlersOfCrutan.Infrastructure;
using SettlersOfCrutan.Infrastructure.Redis;
using SettlersOfCrutan.Infrastructure.SignalR;
using SettlersOfCrutan.Presentation;
using SettlersOfCrutan.Presentation.Endpoints;
using SettlersOfCrutan.Presentation.Identity;
using SettlersOfCrutan.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.AddRedisClient("redis");
builder.AddAzureTableServiceClient("tables");

builder.Services.AddOpenApi();
builder.Services.AddApplicationServices();
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
builder.Services.AddInfrastructureServices();
builder.Services.AddSignalR().AddStackExchangeRedis(builder.Configuration.GetConnectionString("redis")!);

// Flatten BaseId value objects in HTTP JSON (responses and requests)
builder.Services.AddHttpJsonSettings();
builder.Services.AddApplicationCookie();
builder.Services.AddApplicationIdentity();

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsProduction())
{
}

if (app.Environment.IsDevelopment())
{
    await app.Services.SeedDefaultUserWithAdminRoleAsync("example@gmail.com", "Password123");
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGameViewEndpoints();
app.MapGamePlayEndpoints();
app.MapGameBuildEndpoints();
app.MapGameTradeEndpoints();
app.MapGameDevelopmentCardEndpoints();
app.MapGameTurnFlowEndpoints();
app.MapAuthEndpoints();

app.MapHub<GameHub>("/hubs/game");

app.Run();