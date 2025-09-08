using Azure.Data.Tables;
using ElCamino.AspNetCore.Identity.AzureTable;
using ElCamino.AspNetCore.Identity.AzureTable.Model;

namespace SettlersOfCrutan.Presentation.Identity;

public static class IdentityExtensions
{
    public static IServiceCollection AddApplicationIdentity(this IServiceCollection services)
    {
        services
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
        return services;
    }
}
