using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;

namespace PostForge.Providers.TikTok;

public static class DependencyInjection
{
    public static IServiceCollection AddTikTokProvider(this IServiceCollection services)
    {
        services.AddScoped<ISocialPlatformProvider, TikTokProvider>();

        services.AddHttpClient("TikTokProvider")
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://open-api.tiktok.com/"));

        return services;
    }
}