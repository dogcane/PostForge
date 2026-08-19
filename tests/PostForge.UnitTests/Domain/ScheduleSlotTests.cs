using FluentAssertions;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;

namespace PostForge.UnitTests.Domain;

public class ScheduleSlotTests
{
    private readonly Guid TenantId = Guid.NewGuid();
    private readonly Guid _postId = Guid.NewGuid();
    private readonly DateTime _futureDate = DateTime.UtcNow.AddDays(1);

    [Fact]
    public void CreatingSlot_ShouldHaveStatusScheduled()
    {
        var result = ScheduleSlot.Create(TenantId, _postId, "FACEBOOK", _futureDate);

        result.Success.Should().BeTrue();
        result.Value!.Status.Should().Be(PostStatus.Scheduled);
    }

    [Fact]
    public void CreatingSlot_ShouldHaveRetryCountZero()
    {
        var result = ScheduleSlot.Create(TenantId, _postId, "FACEBOOK", _futureDate);

        result.Value!.RetryCount.Should().Be(0);
    }

    [Fact]
    public void CreatingSlot_WithEmptyPostId_ShouldReturnFailure()
    {
        var result = ScheduleSlot.Create(TenantId, Guid.Empty, "FACEBOOK", _futureDate);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "PostId");
    }

    [Fact]
    public void CreatingSlot_WithNonUtcDate_ShouldReturnFailure()
    {
        var result = ScheduleSlot.Create(TenantId, _postId, "FACEBOOK", DateTime.Now);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "ScheduledAt");
    }

    [Fact]
    public void MarkPublished_ShouldSetStatusToPublished()
    {
        var slot = ScheduleSlot.Create(TenantId, _postId, "FACEBOOK", _futureDate).Value!;

        var result = slot.MarkPublished();

        result.Success.Should().BeTrue();
        slot.Status.Should().Be(PostStatus.Published);
    }

    [Fact]
    public void MarkPublished_WhenAlreadyPublished_ShouldReturnFailure()
    {
        var slot = ScheduleSlot.Create(TenantId, _postId, "FACEBOOK", _futureDate).Value!;
        slot.MarkPublished();

        var result = slot.MarkPublished();

        result.Success.Should().BeFalse();
    }

    [Fact]
    public void MarkFailed_ShouldSetStatusToFailedAndIncrementRetryCount()
    {
        var slot = ScheduleSlot.Create(TenantId, _postId, "FACEBOOK", _futureDate).Value!;

        var result = slot.MarkFailed("Network error");

        result.Success.Should().BeTrue();
        slot.Status.Should().Be(PostStatus.Failed);
        slot.RetryCount.Should().Be(1);
        slot.LastError.Should().Be("Network error");
    }

    [Fact]
    public void CanRetry_ShouldReturnTrueWhenRetryCountIsZero()
    {
        var slot = ScheduleSlot.Create(TenantId, _postId, "FACEBOOK", _futureDate).Value!;

        slot.CanRetry.Should().BeTrue();
    }

    [Fact]
    public void CanRetry_ShouldReturnTrueAfterOneRetry()
    {
        var slot = ScheduleSlot.Create(TenantId, _postId, "FACEBOOK", _futureDate).Value!;
        slot.MarkFailed();

        slot.CanRetry.Should().BeTrue();
    }

    [Fact]
    public void CanRetry_ShouldReturnTrueAfterTwoRetries()
    {
        var slot = ScheduleSlot.Create(TenantId, _postId, "FACEBOOK", _futureDate).Value!;
        slot.MarkFailed();
        slot.MarkFailed();

        slot.CanRetry.Should().BeTrue();
    }

    [Fact]
    public void CanRetry_ShouldReturnFalseAfterThreeRetries()
    {
        var slot = ScheduleSlot.Create(TenantId, _postId, "FACEBOOK", _futureDate).Value!;
        slot.MarkFailed();
        slot.MarkFailed();
        slot.MarkFailed();

        slot.CanRetry.Should().BeFalse();
    }

    [Fact]
    public void MarkFailedWithoutError_ShouldSetLastErrorToNull()
    {
        var slot = ScheduleSlot.Create(TenantId, _postId, "FACEBOOK", _futureDate).Value!;

        slot.MarkFailed();

        slot.LastError.Should().BeNull();
    }
}
