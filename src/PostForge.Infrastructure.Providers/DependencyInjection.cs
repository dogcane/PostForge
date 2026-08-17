using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;
using PostForge.Infrastructure.Providers.Ai;
using PostForge.Infrastructure.Providers.Social;

namespace PostForge.Infrastructure.Providers;

public static class DependencyInjection
{
    public static IServiceCollection AddProviderRegistries(this IServiceCollection services)
    {
        services.AddScoped<ISocialPlatformProviderRegistry, SocialPlatformProviderRegistry>();
        services.AddScoped<IProviderRegistry<IAiTextProvider>, AiTextProviderRegistry>();
        services.AddScoped<IProviderRegistry<IAiImageProvider>, AiImageProviderRegistry>();

        return services;
    }
}