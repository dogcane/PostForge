namespace PostForge.Infrastructure.Messaging;

public interface IPublishJobSender
{
    Task SendPublishJobAsync(Guid slotId, CancellationToken ct);
}
