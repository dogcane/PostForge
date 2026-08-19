using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;

namespace PostForge.Providers.Fake;

public static class DependencyInjection
{
    public static IServiceCollection AddFakeProvider(this IServiceCollection services)
    {
        services.AddScoped<ISocialPlatformProvider, FakeSocialProvider>();
        services.AddScoped<IAiTextProvider, FakeAiTextProvider>();
        services.AddScoped<IAiImageProvider, FakeAiImageProvider>();

        return services;
    }
}