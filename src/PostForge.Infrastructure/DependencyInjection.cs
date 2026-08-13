using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Providers;
using PostForge.Infrastructure.DAL;
using PostForge.Infrastructure.Messaging;
using PostForge.Infrastructure.Providers.Ai;
using PostForge.Infrastructure.Providers.Social;
using PostForge.Providers.Anthropic;
using PostForge.Providers.DallE;
using PostForge.Providers.Facebook;
using PostForge.Providers.GoogleGemini;
using PostForge.Providers.Instagram;
using PostForge.Providers.MicrosoftFoundry;
using PostForge.Providers.OpenAI;
using PostForge.Providers.StableDiffusion;
using PostForge.Providers.TikTok;
using PostForge.Providers.YouTube;

namespace PostForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataAccess(configuration);

        RegisterSocialProviders(services);
        RegisterAiProviders(services);
        RegisterMessaging(services);

        return services;
    }

    private static void RegisterSocialProviders(IServiceCollection services)
    {
        services.AddFacebookProvider();
        services.AddInstagramProvider();
        services.AddTikTokProvider();
        services.AddYouTubeProvider();

        services.AddScoped<ISocialPlatformProviderRegistry, SocialPlatformProviderRegistry>();
    }

    private static void RegisterAiProviders(IServiceCollection services)
    {
        services.AddOpenAiTextProvider();
        services.AddAnthropicTextProvider();
        services.AddGoogleGeminiTextProvider();
        services.AddMicrosoftFoundryTextProvider();

        services.AddDallEImageProvider();
        services.AddStableDiffusionImageProvider();

        services.AddScoped<IProviderRegistry<IAiTextProvider>, AiTextProviderRegistry>();
        services.AddScoped<IProviderRegistry<IAiImageProvider>, AiImageProviderRegistry>();
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