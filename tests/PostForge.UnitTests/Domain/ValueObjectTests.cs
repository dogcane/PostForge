using FluentAssertions;
using PostForge.Domain.ValueObjects;

namespace PostForge.UnitTests.Domain;

public class ValueObjectTests
{
    [Fact]
    public void TwoPostStatusDraftValues_ShouldBeEqual()
    {
        var status1 = PostStatus.Draft;
        var status2 = PostStatus.Draft;

        status1.Should().Be(status2);
    }

    [Fact]
    public void SocialPlatformFacebook_ShouldNotEqualSocialPlatformInstagram()
    {
        var facebook = SocialPlatform.Facebook;
        var instagram = SocialPlatform.Instagram;

        facebook.Should().NotBe(instagram);
    }

    [Fact]
    public void PostStatusValues_ShouldHaveExpectedNames()
    {
        PostStatus.Draft.ToString().Should().Be("Draft");
        PostStatus.Ready.ToString().Should().Be("Ready");
        PostStatus.Scheduled.ToString().Should().Be("Scheduled");
        PostStatus.Publishing.ToString().Should().Be("Publishing");
        PostStatus.Published.ToString().Should().Be("Published");
        PostStatus.Failed.ToString().Should().Be("Failed");
    }

    [Fact]
    public void SocialPlatformValues_ShouldHaveExpectedNames()
    {
        SocialPlatform.Facebook.ToString().Should().Be("Facebook");
        SocialPlatform.Instagram.ToString().Should().Be("Instagram");
        SocialPlatform.TikTok.ToString().Should().Be("TikTok");
        SocialPlatform.YouTube.ToString().Should().Be("YouTube");
    }

    [Fact]
    public void CampaignGoalValues_ShouldHaveExpectedNames()
    {
        CampaignGoal.Awareness.ToString().Should().Be("Awareness");
        CampaignGoal.Reputation.ToString().Should().Be("Reputation");
        CampaignGoal.LeadGeneration.ToString().Should().Be("LeadGeneration");
    }

    [Fact]
    public void CampaignChannelValues_ShouldHaveExpectedNames()
    {
        CampaignChannel.Organic.ToString().Should().Be("Organic");
        CampaignChannel.Paid.ToString().Should().Be("Paid");
    }

    [Fact]
    public void ProviderCredentialScopeValues_ShouldHaveExpectedNames()
    {
        ProviderCredentialScope.Social.ToString().Should().Be("Social");
        ProviderCredentialScope.AiText.ToString().Should().Be("AiText");
        ProviderCredentialScope.AiImage.ToString().Should().Be("AiImage");
    }
}
