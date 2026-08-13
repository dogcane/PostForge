using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;

namespace PostForge.Providers.OpenAI;

public static class DependencyInjection
{
    public static IServiceCollection AddOpenAiTextProvider(this IServiceCollection services)
    {
        services.AddScoped<IAiTextProvider, OpenAiTextProvider>();
        return services;
    }
}