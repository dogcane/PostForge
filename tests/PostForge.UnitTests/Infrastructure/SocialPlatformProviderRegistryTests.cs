using FluentAssertions;
using Microsoft.Extensions.Options;
using PostForge.Domain.Providers;
using PostForge.Infrastructure.Providers.Social;
using PostForge.Providers.Facebook;
using PostForge.Providers.Instagram;
using PostForge.Providers.TikTok;
using PostForge.Providers.YouTube;

namespace PostForge.UnitTests.Infrastructure;

public class SocialPlatformProviderRegistryTests
{
    private static FacebookProvider CreateFacebookProvider()
        => new(new HttpClient(), Options.Create(new FacebookProviderOptions()));

    private readonly ISocialPlatformProvider[] _providers =
    [
        CreateFacebookProvider(),
        new InstagramProvider(),
        new TikTokProvider(),
        new YouTubeProvider(),
    ];

    private readonly SocialPlatformProviderRegistry _sut;

    public SocialPlatformProviderRegistryTests()
    {
        _sut = new SocialPlatformProviderRegistry(_providers);
    }

    [Theory]
    [InlineData("FACEBOOK")]
    [InlineData("INSTAGRAM")]
    [InlineData("TIKTOK")]
    [InlineData("YOUTUBE")]
    public void ResolveByIdentifier_ShouldReturnMatchingProvider(string identifier)
    {
        var provider = _sut.Resolve(identifier);

        provider.Identifier.Should().Be(identifier);
    }

    [Theory]
    [InlineData("FACEBOOK")]
    [InlineData("instagram")]
    [InlineData("TikTok")]
    [InlineData("youtube")]
    public void ResolveByIdentifier_ShouldBeCaseInsensitive(string identifier)
    {
        var provider = _sut.Resolve(identifier);

        provider.Identifier.Should().Be(identifier.ToUpperInvariant());
    }

    [Fact]
    public void ResolveUnknownIdentifier_ShouldThrowKeyNotFound()
    {
        var act = () => _sut.Resolve("TWITTER");

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void AvailableProviderKeys_ShouldContainAllIdentifiers()
    {
        _sut.AvailableProviderKeys.Should().BeEquivalentTo(
            ["FACEBOOK", "INSTAGRAM", "TIKTOK", "YOUTUBE"]);
    }

    [Fact]
    public void Registry_ShouldAcceptAnyIEnumerableOfProviders()
    {
        var registry = new SocialPlatformProviderRegistry([CreateFacebookProvider()]);

        registry.AvailableProviderKeys.Should().ContainSingle().Which.Should().Be("FACEBOOK");
    }
}