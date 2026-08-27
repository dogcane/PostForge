using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;

namespace PostForge.Providers.YouTube;

public static class DependencyInjection
{
    public static IServiceCollection AddYouTubeProvider(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<YouTubeProviderOptions>()
            .Bind(configuration.GetSection(YouTubeProviderOptions.SectionName));

        services.AddHttpClient<YouTubeProvider>(client =>
        {
            client.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/");
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddScoped<ISocialPlatformProvider>(sp => sp.GetRequiredService<YouTubeProvider>());

        return services;
    }
}