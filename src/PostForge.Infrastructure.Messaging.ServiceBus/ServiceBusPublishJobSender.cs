using Azure.Messaging.ServiceBus;
using PostForge.Infrastructure.Messaging;

namespace PostForge.Infrastructure.Messaging.ServiceBus;

public class ServiceBusPublishJobSender(ServiceBusSender sender) : IPublishJobSender
{
    public async Task SendPublishJobAsync(Guid slotId, CancellationToken ct)
    {
        var message = new ServiceBusMessage
        {
            MessageId = slotId.ToString(),
            Subject = "PublishJob",
            Body = BinaryData.FromObjectAsJson(new { SlotId = slotId })
        };

        await sender.SendMessageAsync(message, ct);
    }
}