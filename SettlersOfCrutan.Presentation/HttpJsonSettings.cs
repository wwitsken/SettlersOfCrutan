using SettlersOfCrutan.Infrastructure.Redis.Serialization;

namespace SettlersOfCrutan.Presentation;

public static class HttpJsonSettings
{
    public static IServiceCollection AddHttpJsonSettings(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
            ApiSharedJsonOptions.ApplyTo(options.SerializerOptions));
        return services;
    }
}
