using FluentAssertions;
using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.UnitTests.Domain;

public class ISocialPlatformProviderSupportsTests
{
    private sealed class TestSocialProvider : ISocialPlatformProvider
    {
        public TestSocialProvider(SocialPlatformCapabilities capabilities) => Capabilities = capabilities;

        public string Name => "Test";
        public string Identifier => "TEST";
        public SocialPlatformCapabilities Capabilities { get; }

        public Task<OAuthTokens> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct) => throw new NotImplementedException();
        public Task<OAuthTokens> RefreshTokenAsync(OAuthTokens tokens, CancellationToken ct) => throw new NotImplementedException();
        public Task<PublishResult> PublishAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct) => throw new NotImplementedException();
        public Task<PostInsights?> GetInsightsAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct) => throw new NotImplementedException();
    }

    [Fact]
    public void Supports_DefaultImplementation_ShouldReflectCapabilities()
    {
        ISocialPlatformProvider provider = new TestSocialProvider(SocialPlatformCapabilities.Photo | SocialPlatformCapabilities.Collaborators);

        provider.Supports(SocialPlatformCapabilities.Photo).Should().BeTrue();
        provider.Supports(SocialPlatformCapabilities.Collaborators).Should().BeTrue();
        provider.Supports(SocialPlatformCapabilities.Video).Should().BeFalse();
    }
}