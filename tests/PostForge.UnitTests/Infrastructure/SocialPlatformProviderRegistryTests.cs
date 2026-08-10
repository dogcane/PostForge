using FluentAssertions;
using PostForge.Domain.ValueObjects;
using PostForge.Infrastructure;
using PostForge.Infrastructure.Providers.Social;

namespace PostForge.UnitTests.Infrastructure;

public class SocialPlatformProviderRegistryTests
{
    private readonly ISocialPlatformProvider[] _providers =
    [
        new FacebookProvider(),
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
    [InlineData(SocialPlatform.Facebook, "FACEBOOK")]
    [InlineData(SocialPlatform.Instagram, "INSTAGRAM")]
    [InlineData(SocialPlatform.TikTok, "TIKTOK")]
    [InlineData(SocialPlatform.YouTube, "YOUTUBE")]
    public void ResolveByPlatform_ShouldReturnMatchingProvider(SocialPlatform platform, string identifier)
    {
        var provider = _sut.Resolve(platform);

        provider.Platform.Should().Be(platform);
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
    public void ResolveUnknownPlatform_ShouldThrowKeyNotFound()
    {
        var act = () => _sut.Resolve((SocialPlatform)99);

        act.Should().Throw<KeyNotFoundException>();
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
        var registry = new SocialPlatformProviderRegistry([new FacebookProvider()]);

        registry.AvailableProviderKeys.Should().ContainSingle().Which.Should().Be("FACEBOOK");
    }
}
