using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostForge.Infrastructure.DAL;
using PostForge.Infrastructure.Messaging;
using PostForge.Infrastructure.Providers.Ai;
using PostForge.Infrastructure.Providers.Social;

namespace PostForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataAccess(configuration);

        RegisterSocialProviders(services);
        RegisterAiProviders(services);
        RegisterHttpClients(services);
        RegisterMessaging(services);

        return services;
    }

    private static void RegisterSocialProviders(IServiceCollection services)
    {
        services.AddScoped<ISocialPlatformProvider, FacebookProvider>();
        services.AddScoped<ISocialPlatformProvider, InstagramProvider>();
        services.AddScoped<ISocialPlatformProvider, TikTokProvider>();
        services.AddScoped<ISocialPlatformProvider, YouTubeProvider>();

        services.AddScoped<ISocialPlatformProviderRegistry, SocialPlatformProviderRegistry>();
    }

    private static void RegisterAiProviders(IServiceCollection services)
    {
        services.AddScoped<IAiTextProvider, OpenAiTextProvider>();
        services.AddScoped<IAiTextProvider, AnthropicTextProvider>();
        services.AddScoped<IAiTextProvider, GoogleGeminiTextProvider>();
        services.AddScoped<IAiTextProvider, MicrosoftFoundryTextProvider>();

        services.AddScoped<IAiImageProvider, DallEImageProvider>();
        services.AddScoped<IAiImageProvider, StableDiffusionImageProvider>();

        services.AddScoped<IProviderRegistry<IAiTextProvider>, AiTextProviderRegistry>();
        services.AddScoped<IProviderRegistry<IAiImageProvider>, AiImageProviderRegistry>();
    }

    private static void RegisterHttpClients(IServiceCollection services)
    {
        services.AddHttpClient("FacebookProvider")
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://graph.facebook.com/v22.0/"));

        services.AddHttpClient("InstagramProvider")
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://graph.facebook.com/v22.0/"));

        services.AddHttpClient("TikTokProvider")
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://open-api.tiktok.com/"));

        services.AddHttpClient("YouTubeProvider")
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/"));
    }

    private static void RegisterMessaging(IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var serviceBusConnectionString = config.GetConnectionString("ServiceBus")
                ?? throw new InvalidOperationException("Connection string 'ServiceBus' not found.");

            var client = new ServiceBusClient(serviceBusConnectionString);
            return client.CreateSender("publish-jobs");
        });

        services.AddScoped<IPublishJobSender, ServiceBusPublishJobSender>();
    }
}
