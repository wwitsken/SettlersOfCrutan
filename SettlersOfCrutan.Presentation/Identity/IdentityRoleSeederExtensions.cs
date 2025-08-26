using Microsoft.AspNetCore.Identity;

namespace SettlersOfCrutan.Presentation.Identity;

public static class IdentityRoleSeederExtensions
{
    private const string _roleName = "Admin";

    public static async Task SeedDefaultUserWithAdminRoleAsync(
        this IServiceProvider serviceProvider,
        string userEmail,
        string password)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        //var secretManager = scope.ServiceProvider.GetRequiredService<SecretClient>();

        // Ensure role exists
        if (!await roleManager.RoleExistsAsync(_roleName)) await roleManager.CreateAsync(new ApplicationRole { Name = _roleName });

        // Check if user exists
        var user = await userManager.FindByEmailAsync(userEmail);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        if (!await userManager.IsInRoleAsync(user, _roleName)) await userManager.AddToRoleAsync(user, _roleName);

        //await secretManager.SetSecretAsync(new KeyVaultSecret("DefaultAdminPassword", password));
        //await secretManager.SetSecretAsync(new KeyVaultSecret("DefaultAdminEmail", userEmail));
    }

    public static async Task<string> SeedDefaultUserWithAdminRoleAsync(
        this IServiceProvider serviceProvider,
        string userEmail)
    {
        var password = RandomPasswordGenerator.Generate();
        await serviceProvider.SeedDefaultUserWithAdminRoleAsync(userEmail, password);
        return password;
    }
}