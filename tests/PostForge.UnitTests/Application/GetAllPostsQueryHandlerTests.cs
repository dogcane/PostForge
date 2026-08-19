using FluentAssertions;
using PostForge.Application.Posts.DTOs;
using PostForge.Application.Posts.Queries.GetAllPosts;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;
using PostForge.Infrastructure.DAL.Repositories;

namespace PostForge.UnitTests.Application;

public class GetAllPostsQueryHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_ShouldReturnAllPostsWhenNoFilter()
    {
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(Post.Create("Post 1", TenantId).Value!);
        ctx.Set<Post>().Add(Post.Create("Post 2", TenantId).Value!);
        ctx.Set<Post>().Add(Post.Create("Post 3", TenantId).Value!);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPostsHandler(PostRepository);
        var query = new GetAllPostsQuery(null, null, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_ShouldFilterByStatus()
    {
        var draftPost = Post.Create("Draft post", TenantId).Value!;
        var readyPost = Post.Create("Ready post", TenantId).Value!;
        readyPost.SetStatus(PostStatus.Ready);
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(draftPost);
        ctx.Set<Post>().Add(readyPost);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPostsHandler(PostRepository);
        var query = new GetAllPostsQuery(PostStatus.Draft, null, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().ContainSingle().Which.Id.Should().Be(draftPost.Id);
    }

    [Fact]
    public async Task Handle_ShouldFilterByPlatform()
    {
        var fbPost = Post.Create("Facebook post", TenantId).Value!;
        fbPost.ScheduleForPlatform("FACEBOOK");
        var igPost = Post.Create("Instagram post", TenantId).Value!;
        igPost.ScheduleForPlatform("INSTAGRAM");
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(fbPost);
        ctx.Set<Post>().Add(igPost);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPostsHandler(PostRepository);
        var query = new GetAllPostsQuery(null, "FACEBOOK", null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().ContainSingle().Which.Id.Should().Be(fbPost.Id);
    }

    [Fact]
    public async Task Handle_ShouldFilterByDateFrom()
    {
        var oldPost = Post.Create("Old post", TenantId).Value!;
        var newPost = Post.Create("New post", TenantId).Value!;
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(oldPost);
        ctx.Set<Post>().Add(newPost);
        await ctx.SaveChangesAsync(CancellationToken.None);

        ctx.Entry(oldPost).Property(p => p.CreatedAtUtc).CurrentValue =
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        ctx.Entry(newPost).Property(p => p.CreatedAtUtc).CurrentValue =
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPostsHandler(PostRepository);
        var query = new GetAllPostsQuery(null, null, new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc), null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().ContainSingle().Which.Id.Should().Be(newPost.Id);
    }

    [Fact]
    public async Task Handle_ShouldFilterByDateTo()
    {
        var oldPost = Post.Create("Old post", TenantId).Value!;
        var newPost = Post.Create("New post", TenantId).Value!;
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(oldPost);
        ctx.Set<Post>().Add(newPost);
        await ctx.SaveChangesAsync(CancellationToken.None);

        ctx.Entry(oldPost).Property(p => p.CreatedAtUtc).CurrentValue =
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        ctx.Entry(newPost).Property(p => p.CreatedAtUtc).CurrentValue =
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPostsHandler(PostRepository);
        var query = new GetAllPostsQuery(null, null, null, new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().ContainSingle().Which.Id.Should().Be(oldPost.Id);
    }

[Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoMatches()
    {
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(Post.Create("Post", TenantId).Value!);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPostsHandler(PostRepository);
        var query = new GetAllPostsQuery(PostStatus.Published, null, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldNotReturnPostsFromOtherTenants()
    {
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(Post.Create("My tenant post", TenantId).Value!);
        ctx.Set<Post>().Add(Post.Create("Other tenant post", Guid.NewGuid()).Value!);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPostsHandler(PostRepository);
        var query = new GetAllPostsQuery(null, null, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().ContainSingle().Which.Text.Should().Be("My tenant post");
    }

    [Fact]
    public async Task Handle_ShouldMapToPostDto()
    {
        var post = Post.Create("Test content", TenantId).Value!;
        post.ScheduleForPlatform("TIKTOK");
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(post);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPostsHandler(PostRepository);
        var query = new GetAllPostsQuery(null, null, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        var dto = result.Should().ContainSingle().Which;
        dto.Id.Should().Be(post.Id);
        dto.Text.Should().Be("Test content");
        dto.Status.Should().Be(PostStatus.Draft);
        dto.TargetPlatforms.Should().Contain("TIKTOK");
    }

    [Fact]
    public async Task Handle_ShouldMapTagsToDto()
    {
        var post = Post.Create("Test content", TenantId).Value!;
        post.ScheduleForPlatform("FACEBOOK");
        post.AddTag(PostTag.Create("FACEBOOK", PostTagType.Collaborator, "silvia.neri").Value!);
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(post);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPostsHandler(PostRepository);
        var query = new GetAllPostsQuery(null, null, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        var dto = result.Should().ContainSingle().Which;
        dto.Tags.Should().ContainSingle().Which.Should().Match<PostTagDto>(t =>
            t.Platform == "FACEBOOK" && t.TagType == PostTagType.Collaborator && t.Username == "silvia.neri");
    }
}
