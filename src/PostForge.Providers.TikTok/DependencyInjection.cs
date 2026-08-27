using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;

namespace PostForge.Providers.TikTok;

public static class DependencyInjection
{
    public static IServiceCollection AddTikTokProvider(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TikTokProviderOptions>()
            .Bind(configuration.GetSection(TikTokProviderOptions.SectionName));

        services.AddHttpClient<TikTokProvider>(client =>
        {
            client.BaseAddress = new Uri("https://open.tiktokapis.com/");
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddScoped<ISocialPlatformProvider>(sp => sp.GetRequiredService<TikTokProvider>());

        return services;
    }
}