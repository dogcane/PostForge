using FluentAssertions;
using PostForge.Application.Publishing;
using PostForge.Domain.Entities;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.UnitTests.Application;

public class PostPublishSettingsMapperTests
{
    [Fact]
    public void ToPublishSettings_ShouldPartitionTagsByTypeForPlatform()
    {
        var post = Post.Create("Test content").Value!;
        post.ScheduleForPlatform("FACEBOOK");
        post.ScheduleForPlatform("INSTAGRAM");
        post.AddTag(PostTag.Create("FACEBOOK", PostTagType.Mention, "marco.rossi").Value!);
        post.AddTag(PostTag.Create("FACEBOOK", PostTagType.UserTag, "anna.bianchi").Value!);
        post.AddTag(PostTag.Create("FACEBOOK", PostTagType.Collaborator, "silvia.neri").Value!);
        post.AddTag(PostTag.Create("INSTAGRAM", PostTagType.Collaborator, "luigi.verdi").Value!);

        var settings = post.ToPublishSettings("FACEBOOK");

        settings.Should().BeOfType<PublishSettings>();
        settings.MentionedUsernames.Should().ContainSingle().Which.Should().Be("marco.rossi");
        settings.UserTagUsernames.Should().ContainSingle().Which.Should().Be("anna.bianchi");
        settings.CollaboratorUsernames.Should().ContainSingle().Which.Should().Be("silvia.neri");
    }

    [Fact]
    public void ToPublishSettings_WithoutTags_ShouldReturnEmptyLists()
    {
        var post = Post.Create("Test content").Value!;
        post.ScheduleForPlatform("FACEBOOK");

        var settings = post.ToPublishSettings("FACEBOOK");

        settings.MentionedUsernames.Should().BeEmpty();
        settings.UserTagUsernames.Should().BeEmpty();
        settings.CollaboratorUsernames.Should().BeEmpty();
    }
}