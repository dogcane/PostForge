using ECO;
using PostForge.Domain.ValueObjects;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class ScheduleSlot : AggregateRoot<Guid>
{
    #region Fields
    #endregion

    #region Properties
    public Guid Id => Identity;
    public Guid TenantId { get; private set; }
    public Guid PostId { get; private set; }
    public string Platform { get; private set; }
    public DateTime ScheduledAtUtc { get; private set; }
    public PostStatus Status { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public bool CanRetry => RetryCount < 3;
    #endregion

    #region ctor
    private ScheduleSlot() : base(Guid.NewGuid())
    {
        Platform = null!;
    }

    private ScheduleSlot(Guid tenantId, Guid postId, string platform, DateTime scheduledAtUtc) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        PostId = postId;
        Platform = platform;
        ScheduledAtUtc = scheduledAtUtc;
        Status = PostStatus.Scheduled;
        RetryCount = 0;
    }
    #endregion

    #region Methods
    protected static OperationResult Validate(Guid tenantId, Guid postId, string platform, DateTime scheduledAtUtc)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(tenantId, "TenantId").Condition(v => v != Guid.Empty)
            .With(postId, "PostId").Condition(v => v != Guid.Empty)
            .With(platform, "Platform").Required().StringLength(50)
            .With(scheduledAtUtc, "ScheduledAt").Condition(v => v != default)
            .With(scheduledAtUtc, "ScheduledAt").Condition(v => v.Kind == DateTimeKind.Utc);
        return result;
    }

    public static OperationResult<ScheduleSlot> Create(Guid tenantId, Guid postId, string platform, DateTime scheduledAtUtc)
        => Validate(tenantId, postId, platform, scheduledAtUtc)
            .IfSuccessThenReturn<ScheduleSlot>(() => new ScheduleSlot(tenantId, postId, platform, scheduledAtUtc));

    public OperationResult MarkPublished()
        => Status is not PostStatus.Scheduled and not PostStatus.Publishing
            ? OperationResult.MakeFailure(ErrorMessage.Create("Status", $"Cannot publish a slot with status {Status}."))
            : OperationResult.MakeSuccess().IfSuccess(_ =>
            {
                Status = PostStatus.Published;
                PublishedAtUtc = DateTime.UtcNow;
            });

    public OperationResult MarkFailed(string? error = null)
        => Status is not PostStatus.Publishing and not PostStatus.Scheduled and not PostStatus.Failed
            ? OperationResult.MakeFailure(ErrorMessage.Create("Status", $"Cannot mark as failed a slot with status {Status}."))
            : OperationResult.MakeSuccess().IfSuccess(_ =>
            {
                Status = PostStatus.Failed;
                LastError = error;
                RetryCount++;
            });
    #endregion
}
