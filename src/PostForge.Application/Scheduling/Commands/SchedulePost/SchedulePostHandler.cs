using ECO.Data;
using Mediator;
using PostForge.Domain.Interfaces;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Scheduling.Commands.SchedulePost;

public class SchedulePostHandler(
    IPostRepository postRepository,
    IScheduleSlotRepository scheduleSlotRepository,
    IDataContext dataContext) : IRequestHandler<SchedulePostCommand, Guid>
{
    public async ValueTask<Guid> Handle(SchedulePostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.LoadAsync(request.PostId)
            ?? throw new KeyNotFoundException($"Post with Id {request.PostId} was not found.");

        var statusResult = post.SetStatus(PostStatus.Scheduled);
        if (!statusResult.Success)
            throw new InvalidOperationException(
                string.Join("; ", statusResult.Errors.Select(e => $"{e.Context}: {e.Description}")));

        var platformResult = post.ScheduleForPlatform(request.Platform);
        if (!platformResult.Success)
            throw new InvalidOperationException(
                string.Join("; ", platformResult.Errors.Select(e => $"{e.Context}: {e.Description}")));

        var slotResult = ScheduleSlot.Create(request.PostId, request.Platform, request.ScheduledAtUtc);
        if (!slotResult.Success)
            throw new InvalidOperationException(
                string.Join("; ", slotResult.Errors.Select(e => $"{e.Context}: {e.Description}")));

        var slot = slotResult.Value!;
        scheduleSlotRepository.Add(slot);
        postRepository.Update(post);
        await dataContext.SaveChangesAsync(cancellationToken);

        return slot.Id;
    }
}
