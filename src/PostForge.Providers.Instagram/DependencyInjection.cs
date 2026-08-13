using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;

namespace PostForge.Providers.Instagram;

public static class DependencyInjection
{
    public static IServiceCollection AddInstagramProvider(this IServiceCollection services)
    {
        services.AddScoped<ISocialPlatformProvider, InstagramProvider>();

        services.AddHttpClient("InstagramProvider")
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://graph.facebook.com/v22.0/"));

        return services;
    }
}