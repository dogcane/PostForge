using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PostForge.Application.Auth.DTOs;
using PostForge.Application.Posts.Commands.CreatePost;
using PostForge.Application.Posts.DTOs;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;
using PostForge.Infrastructure.DAL;

namespace PostForge.IntegrationTests.Api;

[Collection("PostgreSql")]
public class PostsControllerTests : IAsyncLifetime
{
    private readonly PostForgeWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly string _connectionString;
    private Guid _tenantId;
    private string _token = string.Empty;

    public PostsControllerTests(PostgreSqlContainerFixture fixture)
    {
        _connectionString = fixture.ConnectionString;
        _factory = new PostForgeWebApplicationFactory(_connectionString);
        _client = _factory.CreateClient();
    }

    private PostForgeDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PostForgeDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new PostForgeDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        await _factory.SeedIdentityAsync();

        _tenantId = Guid.NewGuid();
        await using (var context = CreateDbContext())
        {
            context.Tenants.Add(Tenant.Create($"Test Tenant {_tenantId:N}", $"test-{_tenantId:N}").Value!);
            context.Posts.Add(Post.Create("Seeded post 1", _tenantId).Value!);
            context.Posts.Add(Post.Create("Seeded post 2", _tenantId).Value!);
            await context.SaveChangesAsync();
        }

        _token = await LoginAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }

    private async Task<string> LoginAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = PostForgeWebApplicationFactory.SuperUserEmail,
            password = PostForgeWebApplicationFactory.SuperUserPassword
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResultDto>();
        result.Should().NotBeNull();
        return result!.Token;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        request.Headers.Add("X-Tenant-Id", _tenantId.ToString());
        return request;
    }

    [Fact]
    public async Task GetAll_ShouldReturn200AndList()
    {
        var request = CreateRequest(HttpMethod.Get, "/api/v1/posts");
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var posts = await response.Content.ReadFromJsonAsync<List<PostDto>>();
        posts.Should().NotBeNull();
        posts!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAll_ShouldNotReturnPostsFromOtherTenants()
    {
        await using var context = CreateDbContext();
        context.Posts.Add(Post.Create("Other tenant post", Guid.NewGuid()).Value!);
        await context.SaveChangesAsync();

        var request = CreateRequest(HttpMethod.Get, "/api/v1/posts");
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var posts = await response.Content.ReadFromJsonAsync<List<PostDto>>();
        posts.Should().NotBeNull();
        posts!.Should().NotContain(p => p.Text == "Other tenant post");
    }

    [Fact]
    public async Task GetAll_ShouldFilterByStatus()
    {
        await using var context = CreateDbContext();
        var post = Post.Create("Ready post", _tenantId).Value!;
        post.SetStatus(PostStatus.Ready);
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var request = CreateRequest(HttpMethod.Get, $"/api/v1/posts?status={PostStatus.Ready}");
        var response = await _client.SendAsync(request);

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

        var request = CreateRequest(HttpMethod.Post, "/api/v1/posts");
        request.Content = JsonContent.Create(command);
        var response = await _client.SendAsync(request);

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
        var post = Post.Create("Specific post for retrieval", _tenantId).Value!;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var request = CreateRequest(HttpMethod.Get, $"/api/v1/posts/{post.Id}");
        var response = await _client.SendAsync(request);

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

        var request = CreateRequest(HttpMethod.Get, $"/api/v1/posts/{invalidId}");
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithEmptyText_ShouldReturn400()
    {
        var command = new CreatePostCommand("", null, null, null);

        var request = CreateRequest(HttpMethod.Post, "/api/v1/posts");
        request.Content = JsonContent.Create(command);
        var response = await _client.SendAsync(request);

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

        var request = CreateRequest(HttpMethod.Post, "/api/v1/posts");
        request.Content = JsonContent.Create(command);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var postId = await response.Content.ReadFromJsonAsync<Guid>();

        var getRequest = CreateRequest(HttpMethod.Get, $"/api/v1/posts/{postId}");
        var getResponse = await _client.SendAsync(getRequest);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await getResponse.Content.ReadFromJsonAsync<PostDto>();
        dto.Should().NotBeNull();
        dto!.Tags.Should().ContainSingle(t =>
            t.Platform == "FACEBOOK" && t.TagType == PostTagType.Collaborator && t.Username == "silvia.neri");
    }

    [Fact]
    public async Task GetAll_WithoutTenantHeader_ShouldReturn400()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        var response = await _client.GetAsync("/api/v1/posts");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAll_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/posts");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}