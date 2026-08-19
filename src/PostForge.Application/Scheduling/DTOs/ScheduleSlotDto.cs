using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Scheduling.DTOs;

public class ScheduleSlotDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string Platform { get; set; } = null!;
    public DateTime ScheduledAtUtc { get; set; }
    public PostStatus Status { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
}
