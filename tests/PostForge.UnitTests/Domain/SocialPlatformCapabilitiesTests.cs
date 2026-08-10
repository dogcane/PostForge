using FluentAssertions;
using PostForge.Domain.ValueObjects;

namespace PostForge.UnitTests.Domain;

public class SocialPlatformCapabilitiesTests
{
    [Fact]
    public void None_ShouldBeZero()
    {
        ((long)SocialPlatformCapabilities.None).Should().Be(0);
    }

    [Fact]
    public void AllNamedFlags_ShouldBeSingleBits()
    {
        var flags = Enum.GetValues<SocialPlatformCapabilities>()
            .Where(v => v != SocialPlatformCapabilities.None);

        flags.Should().OnlyContain(flag => ((long)flag & ((long)flag - 1)) == 0);
    }

    [Fact]
    public void NamedFlags_ShouldBeDistinct()
    {
        var names = Enum.GetNames<SocialPlatformCapabilities>();
        var values = Enum.GetValues<SocialPlatformCapabilities>();

        names.Length.Should().Be(values.Length);
    }

    [Fact]
    public void CombinedFlags_ShouldSupportHasFlag()
    {
        var combined = SocialPlatformCapabilities.Photo
            | SocialPlatformCapabilities.Video
            | SocialPlatformCapabilities.Collaborators;

        combined.HasFlag(SocialPlatformCapabilities.Photo).Should().BeTrue();
        combined.HasFlag(SocialPlatformCapabilities.Video).Should().BeTrue();
        combined.HasFlag(SocialPlatformCapabilities.Collaborators).Should().BeTrue();
        combined.HasFlag(SocialPlatformCapabilities.Story).Should().BeFalse();
    }

    [Fact]
    public void HighestFlag_ShouldFitInLong()
    {
        var highest = Enum.GetValues<SocialPlatformCapabilities>()
            .Where(v => v != SocialPlatformCapabilities.None)
            .Max(v => (long)v);

        highest.Should().BeLessThan(long.MaxValue);
    }
}
