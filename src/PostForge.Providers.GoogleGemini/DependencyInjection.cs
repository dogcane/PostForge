using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;

namespace PostForge.Providers.GoogleGemini;

public static class DependencyInjection
{
    public static IServiceCollection AddGoogleGeminiTextProvider(this IServiceCollection services)
    {
        services.AddScoped<IAiTextProvider, GoogleGeminiTextProvider>();
        return services;
    }
}