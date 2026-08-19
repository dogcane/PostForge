using ECO.Data;
using Mediator;
using PostForge.Application.Common.Extensions;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;
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

        post.SetStatus(PostStatus.Scheduled).EnsureSuccess();
        post.ScheduleForPlatform(request.Platform).EnsureSuccess();

        var slot = ScheduleSlot.Create(
            post.TenantId,
            request.PostId,
            request.Platform,
            request.ScheduledAtUtc).EnsureSuccess();

        scheduleSlotRepository.Add(slot);
        postRepository.Update(post);
        await dataContext.SaveChangesAsync(cancellationToken);

        return slot.Id;
    }
}