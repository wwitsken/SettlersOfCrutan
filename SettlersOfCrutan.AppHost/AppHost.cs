using Microsoft.Extensions.Hosting;
using SettlersOfCrutan.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");
var storage = builder.AddAzureStorage("storage");
var tables = storage.AddTables("tables");
//var keyVault = builder.AddAzureKeyVault("kv");

var api = builder.AddProject<Projects.SettlersOfCrutan_Presentation>("api")
    .WithReference(redis)
    .WithReference(tables)
    //.WithReference(keyVault)
    .WaitFor(redis)
    .WaitFor(tables);

if (builder.Environment.IsDevelopment())
{
    storage.RunAsEmulator();
    api.WithScalar();
}

var frontend = builder.AddNpmApp("frontend", "../SettlersOfCrutan.Frontend")
    .WithReference(api)
    .WaitFor(api)
    .WithEnvironment("BROWSER", "none")
    .WithHttpEndpoint(env: "VITE_PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
