using Azure.Data.Tables;
using ElCamino.AspNetCore.Identity.AzureTable;
using ElCamino.AspNetCore.Identity.AzureTable.Model;
using Scalar.AspNetCore;
using SettlersOfCrutan.Application;
using SettlersOfCrutan.Infrastructure;
using SettlersOfCrutan.Infrastructure.Redis;
using SettlersOfCrutan.Infrastructure.Redis.Serialization;
using SettlersOfCrutan.Presentation.Endpoints;
using SettlersOfCrutan.Presentation.Identity;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.AddRedisClient("redis");
builder.AddAzureTableServiceClient("tables");
//builder.AddAzureKeyVaultClient("kv");

builder.Services.AddOpenApi();
builder.Services.AddApplicationServices();
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
builder.Services.AddInfrastructureServices();

// Flatten BaseId value objects in HTTP JSON (responses and requests)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); // or new(JsonNamingPolicy.CamelCase)
    options.SerializerOptions.Converters.Add(new BaseIdJsonConverterFactory());
});

builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>(opts =>
    {
        opts.Password.RequireDigit = true;
        opts.Password.RequireLowercase = true;
        opts.Password.RequireUppercase = true;
        opts.Password.RequireNonAlphanumeric = false;
        opts.Password.RequiredLength = 6;
        opts.User.RequireUniqueEmail = true;
    })
    .AddAzureTableStores<IdentityCloudContext>(_ => new IdentityConfiguration(), sp => sp.GetRequiredService<TableServiceClient>())
    .CreateAzureTablesIfNotExists<ApplicationDbContext>();

builder.Services
    .ConfigureApplicationCookie(options =>
    {
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

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

app.MapGameEndpoints();
app.MapAuthEndpoints();
app.MapTodoListEndpoints();

app.Run();