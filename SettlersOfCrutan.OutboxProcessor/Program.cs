using SettlersOfCrutan.Application;
using SettlersOfCrutan.Infrastructure;
using SettlersOfCrutan.Infrastructure.Outbox;
using SettlersOfCrutan.OutboxProcessor;
using SettlersOfCrutan.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

builder.AddRedisClient("redis");

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

builder.Services.AddSingleton<DomainEventPublisher>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
