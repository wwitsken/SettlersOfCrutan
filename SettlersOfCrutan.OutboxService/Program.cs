using SettlersOfCrutan.Application;
using SettlersOfCrutan.Infrastructure;
using SettlersOfCrutan.Infrastructure.Redis;
using SettlersOfCrutan.OutboxService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddRedisClient("redis");
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddScoped<ProcessOutboxMessagesJob>();
builder.Services.AddHostedService<OutboxBackgroundService>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(OutboxBackgroundService.ActivitySourceName));

var host = builder.Build();
host.Run();
