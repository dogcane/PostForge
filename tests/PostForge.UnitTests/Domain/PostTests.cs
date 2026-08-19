using FluentAssertions;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;

namespace PostForge.UnitTests.Domain;

public class PostTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    [Fact]
    public void CreatingPost_ShouldSetStatusToDraft()
    {
        var result = Post.Create("Test content", TenantId);

        result.Success.Should().BeTrue();
        result.Value!.Status.Should().Be(PostStatus.Draft);
    }

    [Fact]
    public void CreatingPost_WithEmptyText_ShouldReturnFailure()
    {
        var result = Post.Create("", TenantId);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Text");
    }

    [Fact]
    public void CreatingPost_WithNullText_ShouldReturnFailure()
    {
        var result = Post.Create(null!, TenantId);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Text");
    }

    [Fact]
    public void CreatingPost_WithTextOver5000Chars_ShouldReturnFailure()
    {
        var result = Post.Create(new string('x', 5001), TenantId);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Text");
    }

    [Fact]
    public void SetStatus_ShouldChangeStatus()
    {
        var post = Post.Create("Test content", TenantId).Value!;

        var result = post.SetStatus(PostStatus.Ready);

        result.Success.Should().BeTrue();
        post.Status.Should().Be(PostStatus.Ready);
    }

    [Fact]
    public void AddMedia_ShouldAddMediaToCollection()
    {
        var post = Post.Create("Test content", TenantId).Value!;
        var media = MediaAsset.Create(TenantId, "https://example.com/image.jpg", "image/jpeg").Value!;

        var result = post.AddMedia(media);

        result.Success.Should().BeTrue();
        post.MediaAssets.Should().ContainSingle().Which.Should().Be(media);
    }

    [Fact]
    public void AddMedia_WithNull_ShouldReturnFailure()
    {
        var post = Post.Create("Test content", TenantId).Value!;

        var result = post.AddMedia(null!);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Media");
    }

    [Fact]
    public void RemoveMedia_ShouldRemoveMediaFromCollection()
    {
        var post = Post.Create("Test content", TenantId).Value!;
        var media = MediaAsset.Create(TenantId, "https://example.com/image.jpg", "image/jpeg").Value!;
        post.AddMedia(media);

        var result = post.RemoveMedia(media);

        result.Success.Should().BeTrue();
        post.MediaAssets.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMedia_WhenMediaNotInCollection_ShouldReturnFailure()
    {
        var post = Post.Create("Test content", TenantId).Value!;
        var media = MediaAsset.Create(TenantId, "https://example.com/image.jpg", "image/jpeg").Value!;

        var result = post.RemoveMedia(media);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Media");
    }

    [Fact]
    public void ScheduleForPlatform_ShouldAddTargetPlatform()
    {
        var post = Post.Create("Test content", TenantId).Value!;

        var result = post.ScheduleForPlatform("FACEBOOK");

        result.Success.Should().BeTrue();
        post.TargetPlatforms.Should().Contain("FACEBOOK");
    }

    [Fact]
    public void ScheduleForPlatform_ShouldNotAddDuplicatePlatform()
    {
        var post = Post.Create("Test content", TenantId).Value!;

        post.ScheduleForPlatform("FACEBOOK");
        post.ScheduleForPlatform("FACEBOOK");

        post.TargetPlatforms.Should().HaveCount(1);
    }

    [Fact]
    public void UpdateText_ShouldUpdateTextAndTimestamp()
    {
        var post = Post.Create("Original text", TenantId).Value!;

        var result = post.UpdateText("Updated text");

        result.Success.Should().BeTrue();
        post.Text.Should().Be("Updated text");
        post.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdateText_WithEmptyText_ShouldReturnFailure()
    {
        var post = Post.Create("Original text", TenantId).Value!;

        var result = post.UpdateText("");

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Text");
    }

    [Fact]
    public void AddTag_ShouldAddTagWhenPlatformIsTargeted()
    {
        var post = Post.Create("Test content", TenantId).Value!;
        post.ScheduleForPlatform("FACEBOOK");
        var tag = PostTag.Create("FACEBOOK", PostTagType.Mention, "marco.rossi").Value!;

        var result = post.AddTag(tag);

        result.Success.Should().BeTrue();
        post.Tags.Should().ContainSingle().Which.Should().Be(tag);
    }

    [Fact]
    public void AddTag_OnUntargetedPlatform_ShouldReturnFailure()
    {
        var post = Post.Create("Test content", TenantId).Value!;
        post.ScheduleForPlatform("INSTAGRAM");
        var tag = PostTag.Create("FACEBOOK", PostTagType.Collaborator, "marco.rossi").Value!;

        var result = post.AddTag(tag);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Platform");
    }

    [Fact]
    public void AddTag_Duplicate_ShouldReturnFailure()
    {
        var post = Post.Create("Test content", TenantId).Value!;
        post.ScheduleForPlatform("FACEBOOK");
        var tag = PostTag.Create("FACEBOOK", PostTagType.UserTag, "marco.rossi").Value!;
        post.AddTag(tag);

        var result = post.AddTag(tag);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Tag");
    }

    [Fact]
    public void AddTag_WithNull_ShouldReturnFailure()
    {
        var post = Post.Create("Test content", TenantId).Value!;
        post.ScheduleForPlatform("FACEBOOK");

        var result = post.AddTag(null!);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Tag");
    }

    [Fact]
    public void SetTags_ShouldReplaceExistingTags()
    {
        var post = Post.Create("Test content", TenantId).Value!;
        post.ScheduleForPlatform("FACEBOOK");
        var original = PostTag.Create("FACEBOOK", PostTagType.Mention, "annamaria.bianchi").Value!;
        post.AddTag(original);

        var replaced = new[]
        {
            PostTag.Create("FACEBOOK", PostTagType.Mention, "carlo.verdi").Value!,
            PostTag.Create("FACEBOOK", PostTagType.Collaborator, "silvia.neri").Value!
        };

        var result = post.SetTags(replaced);

        result.Success.Should().BeTrue();
        post.Tags.Should().HaveCount(2);
        post.Tags.Should().Contain(replaced);
        post.Tags.Should().NotContain(original);
    }

    [Fact]
    public void SetTags_WithDuplicate_ShouldReturnFailure()
    {
        var post = Post.Create("Test content", TenantId).Value!;
        post.ScheduleForPlatform("FACEBOOK");
        var tag = PostTag.Create("FACEBOOK", PostTagType.Mention, "marco.rossi").Value!;

        var result = post.SetTags([tag, tag]);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Tag");
    }

    [Fact]
    public void SetTags_WithPlatformNotTargeted_ShouldReturnFailure()
    {
        var post = Post.Create("Test content", TenantId).Value!;
        post.ScheduleForPlatform("FACEBOOK");
        var tags = new[] { PostTag.Create("INSTAGRAM", PostTagType.Collaborator, "carlo.verdi").Value! };

        var result = post.SetTags(tags);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Platform");
    }

    [Fact]
    public void RemoveTag_ShouldRemoveTagFromCollection()
    {
        var post = Post.Create("Test content", TenantId).Value!;
        post.ScheduleForPlatform("FACEBOOK");
        var tag = PostTag.Create("FACEBOOK", PostTagType.Mention, "marco.rossi").Value!;
        post.AddTag(tag);

        var result = post.RemoveTag(tag);

        result.Success.Should().BeTrue();
        post.Tags.Should().BeEmpty();
    }

    [Fact]
    public void RemoveTag_WhenTagNotInCollection_ShouldReturnFailure()
    {
        var post = Post.Create("Test content", TenantId).Value!;
        post.ScheduleForPlatform("FACEBOOK");
        var tag = PostTag.Create("FACEBOOK", PostTagType.Mention, "marco.rossi").Value!;

        var result = post.RemoveTag(tag);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Tag");
    }
}
