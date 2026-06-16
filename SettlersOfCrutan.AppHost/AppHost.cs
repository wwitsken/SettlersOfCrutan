using Microsoft.Extensions.Hosting;
using SettlersOfCrutan.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("aca-env");

var redis = builder.AddRedis("redis");

var app = builder.AddProject<Projects.SettlersOfCrutan_Presentation>("api")
    .WithReference(redis)
    .WithExternalHttpEndpoints()
    .WaitFor(redis)
    .PublishAsDockerFile();

if (builder.Environment.IsDevelopment())
{
    var frontend = builder
        .AddViteApp("frontend", "../SettlersOfCrutan.Frontend")
        .WithReference(app)
        .WaitFor(app)
        .ExcludeFromManifest();

    app.WithScalar();
}

builder.Build().Run();
