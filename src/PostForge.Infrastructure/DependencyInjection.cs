using Azure.Messaging.ServiceBus;
using ECO.Data;
using ECO.Providers.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Interfaces;
using PostForge.Infrastructure.Messaging;
using PostForge.Infrastructure.Persistence;
using PostForge.Infrastructure.Persistence.Repositories;
using PostForge.Infrastructure.Providers.Ai;
using PostForge.Infrastructure.Providers.Social;

namespace PostForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostForgeDb")
            ?? throw new InvalidOperationException("Connection string 'PostForgeDb' not found.");

        var dbContextOptions = new DbContextOptionsBuilder<PostForgeDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        services.AddSingleton<IPersistenceUnitFactory>(sp =>
        {
            var factory = new PersistenceUnitFactory();
            var unit = new EntityFrameworkPersistenceUnit<PostForgeDbContext>(
                "PostForgeUnit",
                dbContextOptions,
                null);

            unit.AddClass<Domain.Entities.Post, Guid>();
            unit.AddClass<Domain.Entities.Campaign, Guid>();
            unit.AddClass<Domain.Entities.ScheduleSlot, Guid>();
            unit.AddClass<Domain.Entities.SocialAccount, Guid>();
            unit.AddClass<Domain.Entities.ProviderCredential, Guid>();

            factory.AddPersistenceUnit(unit);
            return factory;
        });

        services.AddScoped<IDataContext>(sp =>
            sp.GetRequiredService<IPersistenceUnitFactory>().OpenDataContext());

        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IScheduleSlotRepository, ScheduleSlotRepository>();
        services.AddScoped<ISocialAccountRepository, SocialAccountRepository>();
        services.AddScoped<IProviderCredentialRepository, ProviderCredentialRepository>();

        RegisterSocialProviders(services);
        RegisterAiProviders(services);
        RegisterHttpClients(services);
        RegisterMessaging(services, configuration);

        return services;
    }

    private static void RegisterSocialProviders(IServiceCollection services)
    {
        services.AddScoped<ISocialPlatformProvider, FacebookProvider>();
        services.AddScoped<ISocialPlatformProvider, InstagramProvider>();
        services.AddScoped<ISocialPlatformProvider, TikTokProvider>();
        services.AddScoped<ISocialPlatformProvider, YouTubeProvider>();
    }

    private static void RegisterAiProviders(IServiceCollection services)
    {
        services.AddScoped<IAiTextProvider, OpenAiTextProvider>();
        services.AddScoped<IAiTextProvider, AnthropicTextProvider>();
        services.AddScoped<IAiTextProvider, GoogleGeminiTextProvider>();
        services.AddScoped<IAiTextProvider, MicrosoftFoundryTextProvider>();

        services.AddScoped<IAiImageProvider, DallEImageProvider>();
        services.AddScoped<IAiImageProvider, StableDiffusionImageProvider>();

        services.AddSingleton<IProviderRegistry<IAiTextProvider>, AiTextProviderRegistry>();
        services.AddSingleton<IProviderRegistry<IAiImageProvider>, AiImageProviderRegistry>();
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

    private static void RegisterMessaging(IServiceCollection services, IConfiguration configuration)
    {
        var serviceBusConnectionString = configuration.GetConnectionString("ServiceBus")
            ?? throw new InvalidOperationException("Connection string 'ServiceBus' not found.");

        services.AddSingleton(sp =>
        {
            var client = new ServiceBusClient(serviceBusConnectionString);
            return client.CreateSender("publish-jobs");
        });

        services.AddScoped<IPublishJobSender, ServiceBusPublishJobSender>();
    }
}
