using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;

namespace PostForge.Providers.YouTube;

public static class DependencyInjection
{
    public static IServiceCollection AddYouTubeProvider(this IServiceCollection services)
    {
        services.AddScoped<ISocialPlatformProvider, YouTubeProvider>();

        services.AddHttpClient("YouTubeProvider")
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/"));

        return services;
    }
}