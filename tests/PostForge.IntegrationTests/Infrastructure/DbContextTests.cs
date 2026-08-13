using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;
using PostForge.Infrastructure.DAL;

namespace PostForge.IntegrationTests.Infrastructure;

[Collection("SqlServer")]
public class DbContextTests
{
    private readonly string _connectionString;

    public DbContextTests(SqlServerContainerFixture fixture)
    {
        _connectionString = fixture.ConnectionString;
    }

    private PostForgeDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PostForgeDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        return new PostForgeDbContext(options);
    }

    [Fact]
    public async Task Database_CanBeCreatedAndSeeded()
    {
        using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task Post_CanBeInsertedAndRetrieved()
    {
        using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        var post = Post.Create("Integration test post content").Value!;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var retrieved = await context.Posts.FindAsync(post.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Text.Should().Be("Integration test post content");
        retrieved.Status.Should().Be(PostStatus.Draft);
        retrieved.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task Post_CanBeUpdated()
    {
        using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        var post = Post.Create("Original text").Value!;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        post.UpdateText("Updated text");
        await context.SaveChangesAsync();

        var retrieved = await context.Posts.FindAsync(post.Id);
        retrieved!.Text.Should().Be("Updated text");
        retrieved.UpdatedAtUtc.Should().BeAfter(retrieved.CreatedAtUtc);
    }

    [Fact]
    public async Task Post_CanBeDeleted()
    {
        using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        var post = Post.Create("Post to delete").Value!;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        context.Posts.Remove(post);
        await context.SaveChangesAsync();

        var retrieved = await context.Posts.FindAsync(post.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task Post_CanHaveMediaAssets()
    {
        using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        var post = Post.Create("Post with media").Value!;
        var media = MediaAsset.Create("https://example.com/image.jpg", "image/jpeg").Value!;
        post.AddMedia(media);
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var retrieved = await context.Posts
            .FirstAsync(p => p.Identity == post.Id);
        retrieved.MediaAssets.Should().ContainSingle();
        retrieved.MediaAssets[0].BlobUri.Should().Be("https://example.com/image.jpg");
    }

    [Fact]
    public async Task Post_CanBeScheduledForPlatforms()
    {
        using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        var post = Post.Create("Multi-platform post").Value!;
        post.ScheduleForPlatform("FACEBOOK");
        post.ScheduleForPlatform("INSTAGRAM");
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var retrieved = await context.Posts.FindAsync(post.Id);
        retrieved!.TargetPlatforms.Should().HaveCount(2);
        retrieved.TargetPlatforms.Should().Contain(["FACEBOOK", "INSTAGRAM"]);
    }

    [Fact]
    public async Task Post_CanHaveTags()
    {
        using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        var post = Post.Create("Post with tags").Value!;
        post.ScheduleForPlatform("FACEBOOK");
        post.AddTag(PostTag.Create("FACEBOOK", PostTagType.Mention, "marco.rossi").Value!);
        post.AddTag(PostTag.Create("FACEBOOK", PostTagType.Collaborator, "silvia.neri").Value!);
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var retrieved = await context.Posts.FindAsync(post.Id);
        retrieved!.Tags.Should().HaveCount(2);
        retrieved.Tags.Should().Contain(t =>
            t.Platform == "FACEBOOK" && t.TagType == PostTagType.Collaborator && t.Username == "silvia.neri");
    }

    [Fact]
    public async Task ScheduleSlot_CanBeCreatedAndLinkedToPost()
    {
        using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        var post = Post.Create("Scheduled post").Value!;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var slot = ScheduleSlot.Create(post.Id, "TIKTOK", DateTime.UtcNow.AddDays(7)).Value!;
        context.ScheduleSlots.Add(slot);
        await context.SaveChangesAsync();

        var retrieved = await context.ScheduleSlots.FindAsync(slot.Id);
        retrieved.Should().NotBeNull();
        retrieved!.PostId.Should().Be(post.Id);
        retrieved.Platform.Should().Be("TIKTOK");
        retrieved.Status.Should().Be(PostStatus.Scheduled);
    }

    [Fact]
    public async Task Campaign_CanBeCreatedWithPosts()
    {
        using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        var campaign = Campaign.Create(
            "Test Campaign",
            CampaignGoal.Awareness,
            CampaignChannel.Organic,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30)).Value!;

        var post1 = Post.Create("Campaign post 1").Value!;
        var post2 = Post.Create("Campaign post 2").Value!;
        context.Posts.Add(post1);
        context.Posts.Add(post2);
        await context.SaveChangesAsync();

        campaign.AddPost(post1.Id);
        campaign.AddPost(post2.Id);
        context.Campaigns.Add(campaign);
        await context.SaveChangesAsync();

        var retrieved = await context.Campaigns.FindAsync(campaign.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Campaign");
        retrieved.PostIds.Should().HaveCount(2);
        retrieved.PostIds.Should().Contain([post1.Id, post2.Id]);
    }

    [Fact]
    public async Task SocialAccount_CanBeStored()
    {
        using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        var account = SocialAccount.Create("FACEBOOK", "My Page", "encrypted_oauth_token").Value!;
        context.SocialAccounts.Add(account);
        await context.SaveChangesAsync();

        var retrieved = await context.SocialAccounts.FindAsync(account.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Platform.Should().Be("FACEBOOK");
        retrieved.DisplayName.Should().Be("My Page");
    }

    [Fact]
    public async Task ProviderCredential_CanBeStoredAndValidated()
    {
        using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        var credential = ProviderCredential.Create(
            "openai",
            ProviderCredentialScope.AiText,
            "kv-ref-openai-key").Value!;
        context.ProviderCredentials.Add(credential);
        await context.SaveChangesAsync();

        credential.MarkAsValidated();
        await context.SaveChangesAsync();

        var retrieved = await context.ProviderCredentials.FindAsync(credential.Id);
        retrieved.Should().NotBeNull();
        retrieved!.ProviderKey.Should().Be("openai");
        retrieved.Scope.Should().Be(ProviderCredentialScope.AiText);
        retrieved.IsValidated.Should().BeTrue();
    }
}
