using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        //group.MapPost("/login", async Task<Results<NoContent, UnauthorizedHttpResult>> (
        //    LoginRequest req,
        //    UserManager<IdentityUser> userManager,
        //    SignInManager<IdentityUser> signInManager) =>
        //{
        //    var user = await userManager.FindByEmailAsync(req.Email);
        //    if (user is null)
        //        return TypedResults.Unauthorized();

        //    var pwOk = await userManager.CheckPasswordAsync(user, req.Password);
        //    if (!pwOk)
        //        return TypedResults.Unauthorized();

        //    await signInManager.SignInAsync(user, isPersistent: false);

        //    return TypedResults.NoContent();
        //});

        //group.MapPost("/logout", async Task<Results<NoContent, UnauthorizedHttpResult>> (
        //    SignInManager<IdentityUser> signInManager) =>
        //{
        //    await signInManager.SignOutAsync();
        //    return TypedResults.NoContent();
        //});

        //group.MapGet("/me", async Task<Results<Ok<UserInfoResponse>, UnauthorizedHttpResult>> (ClaimsPrincipal principal, UserManager<IdentityUser> userManager) =>
        //{
        //    var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (string.IsNullOrEmpty(userId)) return TypedResults.Unauthorized();

        //    var user = await userManager.FindByIdAsync(userId);
        //    if (user is null) return TypedResults.Unauthorized();

        //    IList<string> roles = await userManager.GetRolesAsync(user);

        //    return TypedResults.Ok(new UserInfoResponse(user.Id, user.Email ?? "", roles));
        //});

        //group.MapPost("/change-password", async Task<Results<NoContent, UnauthorizedHttpResult>> (ChangePasswordRequest req, ClaimsPrincipal principal, UserManager<IdentityUser> userManager) =>
        //{
        //    var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (string.IsNullOrEmpty(userId)) return TypedResults.Unauthorized();

        //    var user = await userManager.FindByIdAsync(userId);
        //    if (user is null || !await userManager.CheckPasswordAsync(user, req.CurrentPassword))
        //        return TypedResults.Unauthorized();

        //    await userManager.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);

        //    return TypedResults.NoContent();
        //})
        //.RequireAuthorization();

        //// --- Admin: Create user with temporary password ---
        //group.MapPost("/admin/create-user", async Task<Results<Ok<string>, UnauthorizedHttpResult, ValidationProblem>> (
        //    AdminCreateUserRequest req,
        //    UserManager<IdentityUser> userManager) =>
        //{
        //    var tempPassword = RandomPasswordGenerator.Generate();

        //    var user = new IdentityUser
        //    {
        //        UserName = req.Email,
        //        Email = req.Email,
        //        EmailConfirmed = false
        //    };

        //    IdentityResult result = await userManager.CreateAsync(user, tempPassword);
        //    if (!result.Succeeded)
        //    {
        //        return TypedResults.ValidationProblem(result.Errors
        //            .GroupBy(e => e.Code)
        //            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
        //    }

        //    return TypedResults.Ok(tempPassword);
        //})
        //.RequireAuthorization(policy => policy.RequireRole("Admin"));

        //// --- Admin: Reset user password ---
        //group.MapPost("/admin/reset-password", async Task<Results<NoContent, NotFound, ValidationProblem>> (
        //    AdminResetPasswordRequest req,
        //    UserManager<IdentityUser> userManager) =>
        //{
        //    var user = await userManager.FindByEmailAsync(req.Email);
        //    if (user is null)
        //        return TypedResults.NotFound();

        //    var token = await userManager.GeneratePasswordResetTokenAsync(user);
        //    var result = await userManager.ResetPasswordAsync(user, token, req.NewPassword);

        //    if (!result.Succeeded)
        //    {
        //        return TypedResults.ValidationProblem(result.Errors
        //            .GroupBy(e => e.Code)
        //            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
        //    }

        //    return TypedResults.NoContent();
        //})
        //.RequireAuthorization(policy => policy.RequireRole("Admin"));

        return app;
    }
}

// ------------ DTOs ------------
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record LoginRequest(string Email, string Password);
public record AdminCreateUserRequest(string Email);
public record AdminResetPasswordRequest(string Email, string NewPassword);

public record UserInfoResponse(string UserId, string Email, IList<string> Roles);