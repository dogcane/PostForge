using FluentAssertions;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;

namespace PostForge.UnitTests.Domain;

public class CampaignTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly DateTime FixedDate = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CreatingCampaign_ShouldInitializeWithEmptyPostIds()
    {
        var result = Campaign.Create(TenantId, "Test Campaign", CampaignGoal.Awareness, CampaignChannel.Organic, FixedDate);

        result.Success.Should().BeTrue();
        result.Value!.PostIds.Should().BeEmpty();
    }

    [Fact]
    public void CreatingCampaign_ShouldSetProperties()
    {
        var result = Campaign.Create(TenantId, "Test Campaign", CampaignGoal.Awareness, CampaignChannel.Organic, FixedDate, FixedDate.AddDays(30));

        result.Value!.Name.Should().Be("Test Campaign");
        result.Value.Goal.Should().Be(CampaignGoal.Awareness);
        result.Value.Channel.Should().Be(CampaignChannel.Organic);
        result.Value.StartDateUtc.Should().Be(FixedDate);
        result.Value.EndDateUtc.Should().Be(FixedDate.AddDays(30));
    }

    [Fact]
    public void CreatingCampaign_WithoutEndDate_ShouldSetEndDateToNull()
    {
        var result = Campaign.Create(TenantId, "Test Campaign", CampaignGoal.Awareness, CampaignChannel.Organic, FixedDate);

        result.Value!.EndDateUtc.Should().BeNull();
    }

    [Fact]
    public void CreatingCampaign_WithEmptyName_ShouldReturnFailure()
    {
        var result = Campaign.Create(TenantId, "", CampaignGoal.Awareness, CampaignChannel.Organic, FixedDate);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Name");
    }

    [Fact]
    public void CreatingCampaign_WithEndDateBeforeStartDate_ShouldReturnFailure()
    {
        var result = Campaign.Create(TenantId, "Test", CampaignGoal.Awareness, CampaignChannel.Organic, FixedDate, FixedDate.AddDays(-1));

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "EndDate");
    }

    [Fact]
    public void AddPost_ShouldAddPostIdToCollection()
    {
        var campaign = Campaign.Create(TenantId, "Test Campaign", CampaignGoal.Awareness, CampaignChannel.Organic, FixedDate).Value!;
        var postId = Guid.NewGuid();

        var result = campaign.AddPost(postId);

        result.Success.Should().BeTrue();
        campaign.PostIds.Should().Contain(postId);
    }

    [Fact]
    public void AddPost_WithEmptyGuid_ShouldReturnFailure()
    {
        var campaign = Campaign.Create(TenantId, "Test Campaign", CampaignGoal.Awareness, CampaignChannel.Organic, FixedDate).Value!;

        var result = campaign.AddPost(Guid.Empty);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "PostId");
    }

    [Fact]
    public void AddPost_ShouldNotAddDuplicate()
    {
        var campaign = Campaign.Create(TenantId, "Test Campaign", CampaignGoal.Awareness, CampaignChannel.Organic, FixedDate).Value!;
        var postId = Guid.NewGuid();

        campaign.AddPost(postId);
        campaign.AddPost(postId);

        campaign.PostIds.Should().HaveCount(1);
    }

    [Fact]
    public void RemovePost_ShouldRemovePostIdFromCollection()
    {
        var campaign = Campaign.Create(TenantId, "Test Campaign", CampaignGoal.Awareness, CampaignChannel.Organic, FixedDate).Value!;
        var postId = Guid.NewGuid();
        campaign.AddPost(postId);

        var result = campaign.RemovePost(postId);

        result.Success.Should().BeTrue();
        campaign.PostIds.Should().BeEmpty();
    }

    [Fact]
    public void RemoveNonExistentPost_ShouldReturnFailure()
    {
        var campaign = Campaign.Create(TenantId, "Test Campaign", CampaignGoal.Awareness, CampaignChannel.Organic, FixedDate).Value!;

        var result = campaign.RemovePost(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "PostId");
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateAllProperties()
    {
        var campaign = Campaign.Create(TenantId, "Original", CampaignGoal.Awareness, CampaignChannel.Organic, FixedDate).Value!;

        var result = campaign.UpdateDetails("Updated", CampaignGoal.Reputation, CampaignChannel.Paid, FixedDate.AddDays(1), FixedDate.AddDays(15));

        result.Success.Should().BeTrue();
        campaign.Name.Should().Be("Updated");
        campaign.Goal.Should().Be(CampaignGoal.Reputation);
        campaign.Channel.Should().Be(CampaignChannel.Paid);
        campaign.StartDateUtc.Should().Be(FixedDate.AddDays(1));
        campaign.EndDateUtc.Should().Be(FixedDate.AddDays(15));
    }
}
