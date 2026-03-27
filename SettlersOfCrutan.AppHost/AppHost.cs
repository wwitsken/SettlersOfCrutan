using Aspire.Hosting;
using Microsoft.Extensions.Hosting;
using SettlersOfCrutan.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");
//var authAuthority = builder.AddParameter("Authentication-Authority");
//var authAudience = builder.AddParameter("Authentication-Audience");

// Authority is found in the Azure portal App Registrations under Endpoints
// Audience is found as the Application (Client) ID in the Azure portal App Registrations for the API

var api = builder.AddProject<Projects.SettlersOfCrutan_Presentation>("api")
    .WithReference(redis)
    .WaitFor(redis);

IResourceBuilder<NodeAppResource> frontEnd;

if (builder.Environment.IsDevelopment())
{
    api.WithScalar();

    frontEnd = builder.AddNpmApp("frontend", "../SettlersOfCrutan.Frontend", scriptName: "dev")
        .WithHttpEndpoint(port: 3000, env: "VITE_PORT");
} else
{
    frontEnd = builder.AddNpmApp("frontend", "../SettlersOfCrutan.Frontend")
        .WithHttpEndpoint(env: "VITE_PORT");
}

frontEnd.WithReference(api)
        .WaitFor(api)
        .WithEnvironment("BROWSER", "none")
        //.WithEnvironment("VITE_ENTRA_CLIENT_ID", "0e3d7d0a-3c05-4eee-bf61-3726fa57d4ae")
        //.WithEnvironment("VITE_ENTRA_TENANT_SUBDOMAIN", "crutanwesdev")
        //.WithEnvironment("VITE_ENTRA_TENANT_ID", "8200ecc3-b4fb-4aa3-aee5-2e731f0e2aba")
        //.WithEnvironment("VITE_ENTRA_USER_FLOW", "SignUpSignIn_Dev")
        //.WithEnvironment("VITE_AUTH_AUDIENCE", authAudience)
        .WithExternalHttpEndpoints();

builder.Build().Run();
