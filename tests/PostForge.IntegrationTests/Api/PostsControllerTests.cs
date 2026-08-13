using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PostForge.Application.Posts.Commands.CreatePost;
using PostForge.Application.Posts.DTOs;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;
using PostForge.Infrastructure.DAL;

namespace PostForge.IntegrationTests.Api;

[Collection("SqlServer")]
public class PostsControllerTests : IAsyncLifetime
{
    private readonly PostForgeWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly string _connectionString;

    public PostsControllerTests(SqlServerContainerFixture fixture)
    {
        _connectionString = fixture.ConnectionString;
        _factory = new PostForgeWebApplicationFactory(_connectionString);
        _client = _factory.CreateClient();
    }

    private PostForgeDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PostForgeDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        return new PostForgeDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();

        await using var context = CreateDbContext();
        context.Posts.Add(Post.Create("Seeded post 1").Value!);
        context.Posts.Add(Post.Create("Seeded post 2").Value!);
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task GetAll_ShouldReturn200AndList()
    {
        var response = await _client.GetAsync("/api/v1/posts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var posts = await response.Content.ReadFromJsonAsync<List<PostDto>>();
        posts.Should().NotBeNull();
        posts!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAll_ShouldFilterByStatus()
    {
        await using var context = CreateDbContext();
        var post = Post.Create("Ready post").Value!;
        post.SetStatus(PostStatus.Ready);
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var response = await _client.GetAsync($"/api/v1/posts?status={PostStatus.Ready}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var posts = await response.Content.ReadFromJsonAsync<List<PostDto>>();
        posts.Should().NotBeNull();
        posts!.Should().AllSatisfy(p => p.Status.Should().Be(PostStatus.Ready));
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturn201AndPostId()
    {
        var command = new CreatePostCommand(
            "New integration test post",
            null,
            new List<string> { "FACEBOOK" },
            null);

        var response = await _client.PostAsJsonAsync("/api/v1/posts", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var postId = await response.Content.ReadFromJsonAsync<Guid>();
        postId.Should().NotBeEmpty();

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.AbsolutePath.Should().Contain(postId.ToString());
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturn200AndPost()
    {
        await using var context = CreateDbContext();
        var post = Post.Create("Specific post for retrieval").Value!;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var response = await _client.GetAsync($"/api/v1/posts/{post.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<PostDto>();
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(post.Id);
        dto.Text.Should().Be("Specific post for retrieval");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturn404()
    {
        var invalidId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/v1/posts/{invalidId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithEmptyText_ShouldReturn400()
    {
        var command = new CreatePostCommand("", null, null, null);

        var response = await _client.PostAsJsonAsync("/api/v1/posts", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithTags_ShouldReturnTagsInGetById()
    {
        var command = new CreatePostCommand(
            "Tagged post",
            null,
            new List<string> { "FACEBOOK" },
            null,
            new List<PostTagDto>
            {
                new("FACEBOOK", PostTagType.Collaborator, "silvia.neri")
            });

        var response = await _client.PostAsJsonAsync("/api/v1/posts", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var postId = await response.Content.ReadFromJsonAsync<Guid>();

        var getResponse = await _client.GetAsync($"/api/v1/posts/{postId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await getResponse.Content.ReadFromJsonAsync<PostDto>();
        dto.Should().NotBeNull();
        dto!.Tags.Should().ContainSingle(t =>
            t.Platform == "FACEBOOK" && t.TagType == PostTagType.Collaborator && t.Username == "silvia.neri");
    }
}
