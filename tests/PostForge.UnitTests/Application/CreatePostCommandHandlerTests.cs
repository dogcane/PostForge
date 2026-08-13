using FluentAssertions;
using FluentValidation;
using PostForge.Application.Posts.Commands.CreatePost;
using PostForge.Application.Posts.DTOs;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;
using PostForge.Infrastructure.DAL.Repositories;

namespace PostForge.UnitTests.Application;

public class CreatePostCommandHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_ShouldCreatePostAndReturnId()
    {
        var handler = new CreatePostHandler(PostRepository, DataContext);
        var command = new CreatePostCommand("Test content", null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        var post = await PostRepository.LoadAsync(result);
        post.Should().NotBeNull();
        post!.Text.Should().Be("Test content");
        post.Status.Should().Be(PostStatus.Draft);
    }

    [Fact]
    public async Task Handle_ShouldAddTargetPlatforms()
    {
        var handler = new CreatePostHandler(PostRepository, DataContext);
        var platforms = new List<string> { "FACEBOOK", "INSTAGRAM" };
        var command = new CreatePostCommand("Test content", null, platforms, null);

        var result = await handler.Handle(command, CancellationToken.None);

        var post = await PostRepository.LoadAsync(result);
        post!.TargetPlatforms.Should().HaveCount(2);
        post.TargetPlatforms.Should().Contain(["FACEBOOK", "INSTAGRAM"]);
    }

    [Fact]
    public async Task Handle_ShouldAddTagsToPost()
    {
        var handler = new CreatePostHandler(PostRepository, DataContext);
        var tags = new List<PostTagDto>
        {
            new("FACEBOOK", PostTagType.Mention, "marco.rossi"),
            new("FACEBOOK", PostTagType.Collaborator, "silvia.neri")
        };
        var command = new CreatePostCommand("Test content", null, ["FACEBOOK"], null, tags);

        var result = await handler.Handle(command, CancellationToken.None);

        var post = await PostRepository.LoadAsync(result);
        post!.Tags.Should().HaveCount(2);
        post.Tags.Should().Contain(t =>
            t.Platform == "FACEBOOK" && t.TagType == PostTagType.Collaborator && t.Username == "silvia.neri");
    }

    [Fact]
    public async Task Handle_WithCampaignId_ShouldAssociateCampaign()
    {
        var handler = new CreatePostHandler(PostRepository, DataContext);
        var campaignId = Guid.NewGuid();
        var command = new CreatePostCommand("Test content", null, null, campaignId);

        var result = await handler.Handle(command, CancellationToken.None);

        var post = await PostRepository.LoadAsync(result);
        post!.CampaignId.Should().Be(campaignId);
    }

    [Fact]
    public async Task Handle_ShouldAddExistingMediaAssets()
    {
        var media = MediaAsset.Create("https://example.com/img.jpg", "image/jpeg").Value!;
        var repo = (PostRepository)PostRepository;
        var ctx = repo.DbContext;
        ctx.Set<MediaAsset>().Add(media);
        await ctx.SaveChangesAsync(CancellationToken.None);

        var handler = new CreatePostHandler(PostRepository, DataContext);
        var command = new CreatePostCommand("Test content", [media.Id], null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        var post = await PostRepository.LoadAsync(result);
        post!.MediaAssets.Should().ContainSingle().Which.Id.Should().Be(media.Id);
    }

    [Fact]
    public async Task Handle_ShouldSavePostToDatabase()
    {
        var handler = new CreatePostHandler(PostRepository, DataContext);
        var command = new CreatePostCommand("Persistent content", null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        var savedPost = await PostRepository.LoadAsync(result);
        savedPost.Should().NotBeNull();
        savedPost!.Text.Should().Be("Persistent content");
    }
}

public class CreatePostValidatorTests
{
    private readonly CreatePostValidator _validator = new();

    [Fact]
    public void Validator_ShouldRejectEmptyText()
    {
        var command = new CreatePostCommand("", null, null, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Text");
    }

    [Fact]
    public void Validator_ShouldRejectTextExceedingMaxLength()
    {
        var command = new CreatePostCommand(new string('x', 5001), null, null, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Text");
    }

    [Fact]
    public void Validator_ShouldAcceptValidCommand()
    {
        var command = new CreatePostCommand("Valid post content", null, null, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ShouldAcceptCommandWithPlatforms()
    {
        var command = new CreatePostCommand("Content", null, ["FACEBOOK"], null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ShouldAcceptCommandWithTagsOnTargetedPlatforms()
    {
        var command = new CreatePostCommand(
            "Content",
            null,
            ["FACEBOOK"],
            null,
            [new PostTagDto("FACEBOOK", PostTagType.Collaborator, "silvia.neri")]);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ShouldRejectTagOnUntargetedPlatform()
    {
        var command = new CreatePostCommand(
            "Content",
            null,
            ["INSTAGRAM"],
            null,
            [new PostTagDto("FACEBOOK", PostTagType.Collaborator, "silvia.neri")]);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Tags");
    }

    [Fact]
    public void Validator_ShouldRejectTagWithInvalidTagType()
    {
        var command = new CreatePostCommand(
            "Content",
            null,
            ["FACEBOOK"],
            null,
            [new PostTagDto("FACEBOOK", (PostTagType)999, "silvia.neri")]);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("TagType"));
    }
}
