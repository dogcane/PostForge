using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostForge.Infrastructure.DAL;
using PostForge.Infrastructure.Identity;
using PostForge.Infrastructure.Messaging.ServiceBus;
using PostForge.Infrastructure.Providers;
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

namespace PostForge.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataAccess(configuration);
        services.AddIdentityInfrastructure(configuration);

        RegisterSocialProviders(services, configuration);
        RegisterAiProviders(services);

        services.AddProviderRegistries();
        services.AddServiceBusPublishJobSender(configuration);

        return services;
    }

    private static void RegisterSocialProviders(IServiceCollection services, IConfiguration configuration)
    {
        services.AddFacebookProvider(configuration);
        services.AddInstagramProvider();
        services.AddTikTokProvider();
        services.AddYouTubeProvider();
    }

    private static void RegisterAiProviders(IServiceCollection services)
    {
        services.AddOpenAiTextProvider();
        services.AddAnthropicTextProvider();
        services.AddGoogleGeminiTextProvider();
        services.AddMicrosoftFoundryTextProvider();

        services.AddDallEImageProvider();
        services.AddStableDiffusionImageProvider();
    }
}