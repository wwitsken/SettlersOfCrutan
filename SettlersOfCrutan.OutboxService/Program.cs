using Quartz;
using SettlersOfCrutan.Application;
using SettlersOfCrutan.Infrastructure;
using SettlersOfCrutan.Infrastructure.Redis;
using SettlersOfCrutan.OutboxService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddRedisClient("redis");
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

// Outbox processing
builder.Services.AddScoped<ProcessOutboxMessagesJob>();

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey(nameof(OutboxPollingQuartzJob));

    q.AddJob<OutboxPollingQuartzJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity($"{nameof(OutboxPollingQuartzJob)}-trigger")
        .StartNow()
        .WithSimpleSchedule(x => x
            .WithInterval(TimeSpan.FromSeconds(1))
            .RepeatForever()));
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

builder.Services.AddSignalR().AddStackExchangeRedis(builder.Configuration.GetConnectionString("redis")!);

var host = builder.Build();
host.Run();
