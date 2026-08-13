using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;

namespace PostForge.Providers.Facebook;

public static class DependencyInjection
{
    public static IServiceCollection AddFacebookProvider(this IServiceCollection services)
    {
        services.AddScoped<ISocialPlatformProvider, FacebookProvider>();

        services.AddHttpClient("FacebookProvider")
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://graph.facebook.com/v22.0/"));

        return services;
    }
}