using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;

namespace PostForge.Providers.Instagram;

public static class DependencyInjection
{
    public static IServiceCollection AddInstagramProvider(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<InstagramProviderOptions>()
            .Bind(configuration.GetSection(InstagramProviderOptions.SectionName));

        services.AddHttpClient<InstagramProvider>(client =>
        {
            client.BaseAddress = new Uri("https://graph.facebook.com/");
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddScoped<ISocialPlatformProvider>(sp => sp.GetRequiredService<InstagramProvider>());

        return services;
    }
}