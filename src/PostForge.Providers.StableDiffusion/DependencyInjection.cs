using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;

namespace PostForge.Providers.StableDiffusion;

public static class DependencyInjection
{
    public static IServiceCollection AddStableDiffusionImageProvider(this IServiceCollection services)
    {
        services.AddScoped<IAiImageProvider, StableDiffusionImageProvider>();
        return services;
    }
}