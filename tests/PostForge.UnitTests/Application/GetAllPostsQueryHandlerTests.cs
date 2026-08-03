using AutoMapper;
using FluentAssertions;
using NSubstitute;
using PostForge.Application.Posts.DTOs;
using PostForge.Application.Posts.Queries.GetAllPosts;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;
using PostForge.Infrastructure.DAL.Repositories;

namespace PostForge.UnitTests.Application;

public class GetAllPostsQueryHandlerTests : HandlerTestBase
{
    private readonly IMapper _mapper;

    public GetAllPostsQueryHandlerTests()
    {
        _mapper = Substitute.For<IMapper>();
        _mapper.Map<List<PostDto>>(Arg.Any<List<Post>>())
            .Returns(args =>
            {
                var posts = (List<Post>)args[0]!;
                return posts.Select(p => new PostDto
                {
                    Id = p.Id,
                    Text = p.Text,
                    MediaAssets = p.MediaAssets.ToList(),
                    TargetPlatforms = p.TargetPlatforms.ToList(),
                    CampaignId = p.CampaignId,
                    Status = p.Status,
                    CreatedAtUtc = p.CreatedAtUtc,
                    UpdatedAtUtc = p.UpdatedAtUtc
                }).ToList();
            });
    }

    [Fact]
    public async Task Handle_ShouldReturnAllPostsWhenNoFilter()
    {
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(Post.Create("Post 1").Value!);
        ctx.Set<Post>().Add(Post.Create("Post 2").Value!);
        ctx.Set<Post>().Add(Post.Create("Post 3").Value!);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPostsHandler(PostRepository, _mapper);
        var query = new GetAllPostsQuery(null, null, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_ShouldFilterByStatus()
    {
        var draftPost = Post.Create("Draft post").Value!;
        var readyPost = Post.Create("Ready post").Value!;
        readyPost.SetStatus(PostStatus.Ready);
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(draftPost);
        ctx.Set<Post>().Add(readyPost);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPostsHandler(PostRepository, _mapper);
        var query = new GetAllPostsQuery(PostStatus.Draft, null, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().ContainSingle().Which.Id.Should().Be(draftPost.Id);
    }

    [Fact]
    public async Task Handle_ShouldFilterByPlatform()
    {
        var fbPost = Post.Create("Facebook post").Value!;
        fbPost.ScheduleForPlatform(SocialPlatform.Facebook);
        var igPost = Post.Create("Instagram post").Value!;
        igPost.ScheduleForPlatform(SocialPlatform.Instagram);
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(fbPost);
        ctx.Set<Post>().Add(igPost);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPostsHandler(PostRepository, _mapper);
        var query = new GetAllPostsQuery(null, SocialPlatform.Facebook, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().ContainSingle().Which.Id.Should().Be(fbPost.Id);
    }

    [Fact]
    public async Task Handle_ShouldFilterByDateFrom()
    {
        var oldPost = Post.Create("Old post").Value!;
        var newPost = Post.Create("New post").Value!;
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(oldPost);
        ctx.Set<Post>().Add(newPost);
        await ctx.SaveChangesAsync(CancellationToken.None);

        ctx.Entry(oldPost).Property(p => p.CreatedAtUtc).CurrentValue =
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        ctx.Entry(newPost).Property(p => p.CreatedAtUtc).CurrentValue =
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPostsHandler(PostRepository, _mapper);
        var query = new GetAllPostsQuery(null, null, new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc), null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().ContainSingle().Which.Id.Should().Be(newPost.Id);
    }

    [Fact]
    public async Task Handle_ShouldFilterByDateTo()
    {
        var oldPost = Post.Create("Old post").Value!;
        var newPost = Post.Create("New post").Value!;
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(oldPost);
        ctx.Set<Post>().Add(newPost);
        await ctx.SaveChangesAsync(CancellationToken.None);

        ctx.Entry(oldPost).Property(p => p.CreatedAtUtc).CurrentValue =
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        ctx.Entry(newPost).Property(p => p.CreatedAtUtc).CurrentValue =
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPostsHandler(PostRepository, _mapper);
        var query = new GetAllPostsQuery(null, null, null, new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().ContainSingle().Which.Id.Should().Be(oldPost.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoMatches()
    {
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(Post.Create("Post").Value!);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPostsHandler(PostRepository, _mapper);
        var query = new GetAllPostsQuery(PostStatus.Published, null, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapToPostDto()
    {
        var post = Post.Create("Test content").Value!;
        post.ScheduleForPlatform(SocialPlatform.TikTok);
        var ctx = ((PostRepository)PostRepository).DbContext;
        ctx.Set<Post>().Add(post);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPostsHandler(PostRepository, _mapper);
        var query = new GetAllPostsQuery(null, null, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        var dto = result.Should().ContainSingle().Which;
        dto.Id.Should().Be(post.Id);
        dto.Text.Should().Be("Test content");
        dto.Status.Should().Be(PostStatus.Draft);
        dto.TargetPlatforms.Should().Contain(SocialPlatform.TikTok);
    }
}
