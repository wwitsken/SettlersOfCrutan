using Microsoft.Extensions.Hosting;
using SettlersOfCrutan.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

var app = builder.AddProject<Projects.SettlersOfCrutan_Presentation>("api")
    .WithReference(redis)
    .WaitFor(redis);

var frontend = builder
    .AddViteApp("frontend", "../SettlersOfCrutan.Frontend")
    .WithReference(app)
    .WaitFor(app)
    .ExcludeFromManifest();

if (builder.Environment.IsDevelopment())
{
    app.WithScalar();
}

app.PublishWithContainerFiles(frontend, "./wwwroot");

builder.Build().Run();
