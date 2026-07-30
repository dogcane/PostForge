using FluentAssertions;
using PostForge.Domain.Entities;
using PostForge.Domain.Events;
using PostForge.Domain.ValueObjects;

namespace PostForge.UnitTests.Domain;

public class PostTests
{
    [Fact]
    public void CreatingPost_ShouldSetStatusToDraft()
    {
        var result = Post.Create("Test content");

        result.Success.Should().BeTrue();
        result.Value!.Status.Should().Be(PostStatus.Draft);
    }

    [Fact]
    public void CreatingPost_ShouldRaisePostCreatedDomainEvent()
    {
        var result = Post.Create("Test content");

        result.Value!.DomainEvents.Should().ContainSingle(e => e is PostCreatedDomainEvent);
    }

    [Fact]
    public void CreatingPost_WithEmptyText_ShouldReturnFailure()
    {
        var result = Post.Create("");

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Text");
    }

    [Fact]
    public void CreatingPost_WithNullText_ShouldReturnFailure()
    {
        var result = Post.Create(null!);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Text");
    }

    [Fact]
    public void CreatingPost_WithTextOver5000Chars_ShouldReturnFailure()
    {
        var result = Post.Create(new string('x', 5001));

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Text");
    }

    [Fact]
    public void SetStatus_ShouldChangeStatusAndRaisePostStatusChangedDomainEvent()
    {
        var post = Post.Create("Test content").Value!;
        post.ClearDomainEvents();

        var result = post.SetStatus(PostStatus.Ready);

        result.Success.Should().BeTrue();
        post.Status.Should().Be(PostStatus.Ready);
        post.DomainEvents.Should().ContainSingle(e => e is PostStatusChangedDomainEvent);
        var domainEvent = post.DomainEvents.OfType<PostStatusChangedDomainEvent>().Single();
        domainEvent.OldStatus.Should().Be(PostStatus.Draft);
        domainEvent.NewStatus.Should().Be(PostStatus.Ready);
    }

    [Fact]
    public void AddMedia_ShouldAddMediaToCollection()
    {
        var post = Post.Create("Test content").Value!;
        var media = MediaAsset.Create("https://example.com/image.jpg", "image/jpeg").Value!;

        var result = post.AddMedia(media);

        result.Success.Should().BeTrue();
        post.MediaAssets.Should().ContainSingle().Which.Should().Be(media);
    }

    [Fact]
    public void AddMedia_WithNull_ShouldReturnFailure()
    {
        var post = Post.Create("Test content").Value!;

        var result = post.AddMedia(null!);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Media");
    }

    [Fact]
    public void RemoveMedia_ShouldRemoveMediaFromCollection()
    {
        var post = Post.Create("Test content").Value!;
        var media = MediaAsset.Create("https://example.com/image.jpg", "image/jpeg").Value!;
        post.AddMedia(media);

        var result = post.RemoveMedia(media);

        result.Success.Should().BeTrue();
        post.MediaAssets.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMedia_WhenMediaNotInCollection_ShouldReturnFailure()
    {
        var post = Post.Create("Test content").Value!;
        var media = MediaAsset.Create("https://example.com/image.jpg", "image/jpeg").Value!;

        var result = post.RemoveMedia(media);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Media");
    }

    [Fact]
    public void ScheduleForPlatform_ShouldAddTargetPlatform()
    {
        var post = Post.Create("Test content").Value!;

        var result = post.ScheduleForPlatform(SocialPlatform.Facebook);

        result.Success.Should().BeTrue();
        post.TargetPlatforms.Should().Contain(SocialPlatform.Facebook);
    }

    [Fact]
    public void ScheduleForPlatform_ShouldNotAddDuplicatePlatform()
    {
        var post = Post.Create("Test content").Value!;

        post.ScheduleForPlatform(SocialPlatform.Facebook);
        post.ScheduleForPlatform(SocialPlatform.Facebook);

        post.TargetPlatforms.Should().HaveCount(1);
    }

    [Fact]
    public void UpdateText_ShouldUpdateTextAndTimestamp()
    {
        var post = Post.Create("Original text").Value!;

        var result = post.UpdateText("Updated text");

        result.Success.Should().BeTrue();
        post.Text.Should().Be("Updated text");
        post.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdateText_WithEmptyText_ShouldReturnFailure()
    {
        var post = Post.Create("Original text").Value!;

        var result = post.UpdateText("");

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Text");
    }
}
