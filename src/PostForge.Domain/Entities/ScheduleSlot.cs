using ECO;
using ECO.Events;
using PostForge.Domain.Events;
using PostForge.Domain.ValueObjects;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class ScheduleSlot : AggregateRoot<Guid>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id => Identity;
    public Guid PostId { get; private set; }
    public SocialPlatform Platform { get; private set; }
    public DateTime ScheduledAtUtc { get; private set; }
    public PostStatus Status { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private ScheduleSlot() : base(Guid.NewGuid()) { }

    private ScheduleSlot(Guid postId, SocialPlatform platform, DateTime scheduledAtUtc) : base(Guid.NewGuid())
    {
        PostId = postId;
        Platform = platform;
        ScheduledAtUtc = scheduledAtUtc;
        Status = PostStatus.Scheduled;
        RetryCount = 0;
    }

    public static OperationResult<ScheduleSlot> Create(Guid postId, SocialPlatform platform, DateTime scheduledAtUtc)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(postId, "PostId").Condition(v => v != Guid.Empty)
            .With(platform, "Platform").Condition(v => Enum.IsDefined(typeof(SocialPlatform), v))
            .With(scheduledAtUtc, "ScheduledAt").Condition(v => v != default)
            .With(scheduledAtUtc, "ScheduledAt").Condition(v => v.Kind == DateTimeKind.Utc);
        if (!result.Success)
            return result;
        return OperationResult<ScheduleSlot>.MakeSuccess(new ScheduleSlot(postId, platform, scheduledAtUtc));
    }

    public OperationResult MarkPublished()
    {
        if (Status != PostStatus.Scheduled && Status != PostStatus.Publishing)
            return OperationResult.MakeFailure(ErrorMessage.Create("Status", $"Cannot publish a slot with status {Status}."));
        Status = PostStatus.Published;
        PublishedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new PostPublishedDomainEvent(PostId, Platform, PublishedAtUtc.Value));
        return OperationResult.MakeSuccess();
    }

    public OperationResult MarkFailed(string? error = null)
    {
        if (Status != PostStatus.Publishing && Status != PostStatus.Scheduled && Status != PostStatus.Failed)
            return OperationResult.MakeFailure(ErrorMessage.Create("Status", $"Cannot mark as failed a slot with status {Status}."));
        Status = PostStatus.Failed;
        LastError = error;
        RetryCount++;
        return OperationResult.MakeSuccess();
    }

    public bool CanRetry => RetryCount < 3;

    public void AddDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void RemoveDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Remove(domainEvent);

    public void ClearDomainEvents() =>
        _domainEvents.Clear();
}
