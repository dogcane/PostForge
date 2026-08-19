using FluentAssertions;
using PostForge.Application.Common.Mappings;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;

namespace PostForge.UnitTests.Application;

public class PostDtoMappingTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    [Fact]
    public void ToDto_ShouldMapTags()
    {
        var post = Post.Create("Test content", TenantId).Value!;
        post.ScheduleForPlatform("FACEBOOK");
        post.AddTag(PostTag.Create("FACEBOOK", PostTagType.Collaborator, "silvia.neri").Value!);

        var dto = post.ToDto();

        dto.Tags.Should().ContainSingle(t =>
            t.Platform == "FACEBOOK"
            && t.TagType == PostTagType.Collaborator
            && t.Username == "silvia.neri");
    }

    [Fact]
    public void ToDto_ShouldMapAllProperties()
    {
        var post = Post.Create("Test content", TenantId).Value!;
        post.ScheduleForPlatform("FACEBOOK");

        var dto = post.ToDto();

        dto.Id.Should().Be(post.Id);
        dto.Text.Should().Be(post.Text);
        dto.TargetPlatforms.Should().BeEquivalentTo(post.TargetPlatforms);
        dto.Status.Should().Be(post.Status);
        dto.CreatedAtUtc.Should().Be(post.CreatedAtUtc);
        dto.UpdatedAtUtc.Should().Be(post.UpdatedAtUtc);
    }
}