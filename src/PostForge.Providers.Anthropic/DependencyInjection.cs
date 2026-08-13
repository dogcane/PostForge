using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;

namespace PostForge.Providers.Anthropic;

public static class DependencyInjection
{
    public static IServiceCollection AddAnthropicTextProvider(this IServiceCollection services)
    {
        services.AddScoped<IAiTextProvider, AnthropicTextProvider>();
        return services;
    }
}