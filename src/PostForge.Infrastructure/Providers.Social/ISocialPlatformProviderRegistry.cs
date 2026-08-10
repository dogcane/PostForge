using PostForge.Domain.ValueObjects;

namespace PostForge.Infrastructure.Providers.Social;

public interface ISocialPlatformProviderRegistry : IProviderRegistry<ISocialPlatformProvider>
{
    ISocialPlatformProvider Resolve(SocialPlatform platform);
}
