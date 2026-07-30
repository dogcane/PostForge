using Azure.Messaging.ServiceBus;

namespace PostForge.Infrastructure.Messaging;

public class ServiceBusPublishJobSender : IPublishJobSender
{
    private readonly ServiceBusSender _sender;

    public ServiceBusPublishJobSender(ServiceBusSender sender)
    {
        _sender = sender;
    }

    public async Task SendPublishJobAsync(Guid slotId, CancellationToken ct)
    {
        var message = new ServiceBusMessage
        {
            MessageId = slotId.ToString(),
            Subject = "PublishJob",
            Body = BinaryData.FromObjectAsJson(new { SlotId = slotId })
        };

        await _sender.SendMessageAsync(message, ct);
    }
}
