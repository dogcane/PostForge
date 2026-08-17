using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;

namespace PostForge.Providers.Facebook;

public static class DependencyInjection
{
    public static IServiceCollection AddFacebookProvider(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<FacebookProviderOptions>()
            .Bind(configuration.GetSection(FacebookProviderOptions.SectionName));

        services.AddHttpClient<FacebookProvider>(client =>
        {
            client.BaseAddress = new Uri("https://graph.facebook.com/");
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddScoped<ISocialPlatformProvider>(sp => sp.GetRequiredService<FacebookProvider>());

        return services;
    }
}