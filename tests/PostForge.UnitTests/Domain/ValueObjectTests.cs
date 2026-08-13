using FluentAssertions;
using PostForge.Domain.ValueObjects;

namespace PostForge.UnitTests.Domain;

public class ValueObjectTests
{
    [Fact]
    public void PostTag_WithSameComponents_ShouldBeEqual()
    {
        var tag1 = PostTag.Create("FACEBOOK", PostTagType.Mention, "marco.rossi").Value!;
        var tag2 = PostTag.Create("FACEBOOK", PostTagType.Mention, "marco.rossi").Value!;

        tag1.Should().Be(tag2);
        (tag1 == tag2).Should().BeTrue();
    }

    [Fact]
    public void PostTag_WithDifferentComponents_ShouldNotBeEqual()
    {
        var tag1 = PostTag.Create("FACEBOOK", PostTagType.Mention, "marco.rossi").Value!;
        var tag2 = PostTag.Create("FACEBOOK", PostTagType.Collaborator, "marco.rossi").Value!;

        tag1.Should().NotBe(tag2);
    }

    [Fact]
    public void PostTag_Create_WithEmptyPlatform_ShouldReturnFailure()
    {
        var result = PostTag.Create("", PostTagType.Mention, "marco.rossi");

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Platform");
    }

    [Fact]
    public void PostTag_Create_WithEmptyUsername_ShouldReturnFailure()
    {
        var result = PostTag.Create("FACEBOOK", PostTagType.Mention, "");

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Username");
    }

    [Fact]
    public void PostTag_Create_WithUndefinedTagType_ShouldReturnFailure()
    {
        var result = PostTag.Create("FACEBOOK", (PostTagType)999, "marco.rossi");

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "TagType");
    }

    [Fact]
    public void TwoPostStatusDraftValues_ShouldBeEqual()
    {
        var status1 = PostStatus.Draft;
        var status2 = PostStatus.Draft;

        status1.Should().Be(status2);
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
