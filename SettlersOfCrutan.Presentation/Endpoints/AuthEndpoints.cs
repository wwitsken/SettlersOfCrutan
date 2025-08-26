using Microsoft.AspNetCore.Identity;
using SettlersOfCrutan.Presentation.Identity;
using System.Security.Claims;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/login", async (
            LoginRequest req,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager) =>
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user is null)
                return Results.Unauthorized();

            var pwOk = await userManager.CheckPasswordAsync(user, req.Password);
            if (!pwOk)
                return Results.Unauthorized();

            await signInManager.SignInAsync(user, isPersistent: false);

            return Results.Ok(new
            {
                Message = "Logged in successfully"
            });
        });

        group.MapPost("/logout", async (
            SignInManager<ApplicationUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Ok(new { Message = "Logged out successfully" });
        });

        group.MapGet("/me", async (ClaimsPrincipal principal, UserManager<ApplicationUser> userManager) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Results.Unauthorized();

            return Results.Ok(new
            {
                user.Id,
                user.Email,
                user.UserName
            });
        })
        .RequireAuthorization();

        group.MapPost("/change-password", async (ChangePasswordRequest req, ClaimsPrincipal principal, UserManager<ApplicationUser> userManager) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user is null || !await userManager.CheckPasswordAsync(user, req.CurrentPassword))
                return Results.Unauthorized();

            await userManager.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);

            return Results.Ok();
        })
        .RequireAuthorization();

        // --- Admin: Create user with temporary password ---
        group.MapPost("/admin/create-user", async (
            AdminCreateUserRequest req,
            UserManager<ApplicationUser> userManager) =>
        {
            var tempPassword = RandomPasswordGenerator.Generate();

            var user = new ApplicationUser
            {
                UserName = req.Email,
                Email = req.Email,
                EmailConfirmed = false
            };

            var result = await userManager.CreateAsync(user, tempPassword);
            if (!result.Succeeded)
            {
                return Results.ValidationProblem(result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
            }

            return Results.Created($"/auth/users/{user.Id}", new { user.Id, user.Email, TemporaryPassword = tempPassword });
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"));

        // --- Admin: Reset user password ---
        group.MapPost("/admin/reset-password", async (
            AdminResetPasswordRequest req,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user is null)
                return Results.NotFound(new { Message = "User not found" });

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, req.NewPassword);

            if (!result.Succeeded)
            {
                return Results.ValidationProblem(result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
            }

            return Results.Ok(new { Message = "Password reset successfully" });
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"));

        return app;
    }
}

// ------------ DTOs ------------
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record LoginRequest(string Email, string Password);
public record AdminCreateUserRequest(string Email);
public record AdminResetPasswordRequest(string Email, string NewPassword);