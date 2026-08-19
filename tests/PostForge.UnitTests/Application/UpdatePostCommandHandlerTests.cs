using FluentAssertions;
using PostForge.Application.Posts.Commands.UpdatePost;
using PostForge.Application.Posts.DTOs;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;
using PostForge.Infrastructure.DAL.Repositories;

namespace PostForge.UnitTests.Application;

public class UpdatePostCommandHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_WithTags_ShouldReplaceExistingTags()
    {
        var post = Post.Create("Original text", TenantId).Value!;
        post.ScheduleForPlatform("FACEBOOK");
        post.AddTag(PostTag.Create("FACEBOOK", PostTagType.Mention, "old.user").Value!);
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(post);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdatePostHandler(PostRepository, DataContext);
        var command = new UpdatePostCommand(
            post.Id,
            "Updated text",
            null,
            ["FACEBOOK"],
            [new PostTagDto("FACEBOOK", PostTagType.Collaborator, "new.user")]);

        await handler.Handle(command, CancellationToken.None);

        var updated = await PostRepository.LoadAsync(post.Id);
        updated.Should().NotBeNull();
        updated!.Text.Should().Be("Updated text");
        updated.Tags.Should().ContainSingle().Which.Username.Should().Be("new.user");
        updated.Tags.Should().NotContain(t => t.Username == "old.user");
    }

    [Fact]
    public async Task Handle_WithoutTags_ShouldKeepExistingTags()
    {
        var post = Post.Create("Original text", TenantId).Value!;
        post.ScheduleForPlatform("FACEBOOK");
        post.AddTag(PostTag.Create("FACEBOOK", PostTagType.Mention, "keep.me").Value!);
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(post);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdatePostHandler(PostRepository, DataContext);
        var command = new UpdatePostCommand(post.Id, "Updated text", null, ["FACEBOOK"]);

        await handler.Handle(command, CancellationToken.None);

        var updated = await PostRepository.LoadAsync(post.Id);
        updated!.Tags.Should().ContainSingle().Which.Username.Should().Be("keep.me");
    }
}