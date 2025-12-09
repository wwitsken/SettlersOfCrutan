namespace SettlersOfCrutan.Presentation;

public static class AppplicationCookie
{
    public static IServiceCollection AddApplicationCookie(this IServiceCollection services)
    {
        services
            .ConfigureApplicationCookie(options =>
            {
                //var baseUri = Environment.GetEnvironmentVariable("FRONTEND_URI");
                //options.AccessDeniedPath = $"{baseUri}/AccessDenied";
                //options.LoginPath = $"{baseUri}/Login";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.SlidingExpiration = true;
            });
        return services;
    }
}
