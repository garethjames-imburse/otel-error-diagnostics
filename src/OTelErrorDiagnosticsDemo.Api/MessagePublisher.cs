using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace OTelErrorDiagnosticsDemo.Api;

public sealed class MessagePublisher
{
    private readonly ILogger<MessagePublisher> _logger;
    private readonly ServiceBusSender _sender;

    public MessagePublisher(
        ILogger<MessagePublisher> logger,
        ServiceBusClient serviceBusClient)
    {
        _logger = logger;
        _sender = serviceBusClient.CreateSender("otelerrordiagnosticsdemo-queue");
    }

    public async Task PublishMessageAsync<TMessage>(TMessage message, CancellationToken cancellationToken) where TMessage : class
    {
        var serviceBusMessageBody = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(serviceBusMessageBody)
        {
            MessageId = Guid.NewGuid().ToString(),
            ApplicationProperties =
            {
                ["MessageType"] = typeof(TMessage).FullName
            }
        };

        _logger.LogInformation("Publishing message: {MessageId}", serviceBusMessage.MessageId);

        await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }
}
