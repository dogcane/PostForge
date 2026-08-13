using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;

namespace PostForge.Providers.DallE;

public static class DependencyInjection
{
    public static IServiceCollection AddDallEImageProvider(this IServiceCollection services)
    {
        services.AddScoped<IAiImageProvider, DallEImageProvider>();
        return services;
    }
}