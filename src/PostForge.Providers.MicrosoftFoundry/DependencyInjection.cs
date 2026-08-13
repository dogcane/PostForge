using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;

namespace PostForge.Providers.MicrosoftFoundry;

public static class DependencyInjection
{
    public static IServiceCollection AddMicrosoftFoundryTextProvider(this IServiceCollection services)
    {
        services.AddScoped<IAiTextProvider, MicrosoftFoundryTextProvider>();
        return services;
    }
}