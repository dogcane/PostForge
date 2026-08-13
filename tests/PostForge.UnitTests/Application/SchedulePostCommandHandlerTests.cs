using FluentAssertions;
using FluentValidation;
using PostForge.Application.Scheduling.Commands.SchedulePost;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;
using PostForge.Infrastructure.DAL.Repositories;

namespace PostForge.UnitTests.Application;

public class SchedulePostCommandHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_ShouldCreateScheduleSlotAndSetPostStatusToScheduled()
    {
        var post = Post.Create("Test content").Value!;
        PostRepository.Add(post);
        await DataContext.SaveChangesAsync(CancellationToken.None);

        var handler = new SchedulePostHandler(PostRepository, ScheduleSlotRepository, DataContext);
        var futureDate = DateTime.UtcNow.AddDays(1);
        var command = new SchedulePostCommand(post.Id, "FACEBOOK", futureDate);

        var slotId = await handler.Handle(command, CancellationToken.None);

        slotId.Should().NotBeEmpty();
        var slot = await ScheduleSlotRepository.LoadAsync(slotId);
        slot.Should().NotBeNull();
        slot!.PostId.Should().Be(post.Id);
        slot.Platform.Should().Be("FACEBOOK");
        slot.ScheduledAtUtc.Should().Be(futureDate);
        slot.Status.Should().Be(PostStatus.Scheduled);

        var updatedPost = await PostRepository.LoadAsync(post.Id);
        updatedPost!.Status.Should().Be(PostStatus.Scheduled);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenPostNotFound()
    {
        var handler = new SchedulePostHandler(PostRepository, ScheduleSlotRepository, DataContext);
        var command = new SchedulePostCommand(Guid.NewGuid(), "FACEBOOK", DateTime.UtcNow.AddDays(1));

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_ShouldAddPlatformToPostTargetPlatforms()
    {
        var post = Post.Create("Test content").Value!;
        PostRepository.Add(post);
        await DataContext.SaveChangesAsync(CancellationToken.None);

        var handler = new SchedulePostHandler(PostRepository, ScheduleSlotRepository, DataContext);
        var command = new SchedulePostCommand(post.Id, "INSTAGRAM", DateTime.UtcNow.AddDays(1));

        await handler.Handle(command, CancellationToken.None);

        var updatedPost = await PostRepository.LoadAsync(post.Id);
        updatedPost!.TargetPlatforms.Should().Contain("INSTAGRAM");
    }
}

public class SchedulePostValidatorTests
{
    private readonly SchedulePostValidator _validator = new();

    [Fact]
    public void Validator_ShouldRejectEmptyPostId()
    {
        var command = new SchedulePostCommand(Guid.Empty, "FACEBOOK", DateTime.UtcNow.AddDays(1));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PostId");
    }

    [Fact]
    public void Validator_ShouldRejectEmptyPlatform()
    {
        var command = new SchedulePostCommand(Guid.NewGuid(), "", DateTime.UtcNow.AddDays(1));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Platform");
    }

    [Fact]
    public void Validator_ShouldRejectPastDates()
    {
        var command = new SchedulePostCommand(Guid.NewGuid(), "FACEBOOK", DateTime.UtcNow.AddDays(-1));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ScheduledAtUtc");
    }

    [Fact]
    public void Validator_ShouldAcceptValidCommand()
    {
        var command = new SchedulePostCommand(Guid.NewGuid(), "FACEBOOK", DateTime.UtcNow.AddDays(1));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
