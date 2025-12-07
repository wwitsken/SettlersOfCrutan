namespace SettlersOfCrutan.Presentation;

public static class AppplicationCookie
{
    public static IServiceCollection AddApplicationCookie(this IServiceCollection services)
    {
        services
            .ConfigureApplicationCookie(options =>
            {
                //options.Events.OnRedirectToLogin = context =>
                //{
                //    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                //    return Task.CompletedTask;
                //};
                //options.Events.OnRedirectToAccessDenied = context =>
                //{
                //    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                //    return Task.CompletedTask;
                //};
                options.AccessDeniedPath = "/AccessDenied";
                options.LoginPath = "/Login";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.SlidingExpiration = true;
            });
        return services;
    }
}
