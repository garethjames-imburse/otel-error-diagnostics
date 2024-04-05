using System.Reflection;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using OpenTelemetry.Trace;
using OTelErrorDiagnosticsDemo.Contract;
using OTelErrorDiagnosticsDemo.Contract.Commands;

namespace OTelErrorDiagnosticsDemo.Worker.Messaging;

public sealed class MessageConsumer
{
    private readonly ILogger<MessageConsumer> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MessageConsumer(
        ILogger<MessageConsumer> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task ConsumeMessageAsync(ProcessMessageEventArgs args)
    {
        using (var activity = Diagnostics.MyActivitySource.StartActivity(nameof(ConsumeMessageAsync)))
        {
            try
            {
                _logger.LogInformation("Consuming message: {MessageId}", args.Message.MessageId);

                var messageTypeName = GetMessageTypeName(args.Message);

                if (activity is not null)
                    activity.DisplayName = $"consume {messageTypeName}";

                using var serviceScope = _serviceScopeFactory.CreateScope();

                var message = GetMessage(messageTypeName, args.Message.Body);
                var messageHandler = GetMessageHandler(message.GetType(), serviceScope);

                await messageHandler.HandleMessageAsync(message, args.CancellationToken);
                await args.CompleteMessageAsync(args.Message, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while consuming message: {MessageId}", args.Message.MessageId);

                await args.DeadLetterMessageAsync(args.Message, ex.Message, ex.ToString(), CancellationToken.None);

                if (activity is not null)
                {
                    activity.SetStatus(Status.Error.WithDescription("Something went wrong!"));
                    activity.RecordException(ex);
                }
            }
        }
    }

    private static string GetMessageTypeName(ServiceBusReceivedMessage serviceBusReceivedMessage)
    {
        if (!serviceBusReceivedMessage.ApplicationProperties.TryGetValue("MessageType", out var messageTypeValue)
            || messageTypeValue is not string messageTypeName)
            throw new InvalidOperationException("Message type not found");

        return messageTypeName;
    }

    private static IMessage GetMessage(string messageTypeName, BinaryData messageBody)
    {
        var messageType = (Assembly.GetAssembly(typeof(MyCommand))?.GetType(messageTypeName, throwOnError: false))
            ?? throw new InvalidOperationException("Message type not found");

        var message = JsonSerializer.Deserialize(messageBody.ToStream(), messageType) as IMessage
            ?? throw new InvalidOperationException("Message deserialization failed");

        return message;
    }

    private static IMessageHandler<IMessage> GetMessageHandler(Type messageType, IServiceScope serviceScope)
    {
        var messageHandlerServiceType = typeof(IMessageHandler<>).MakeGenericType(messageType);
        var messageHandlerService = serviceScope.ServiceProvider.GetRequiredService(messageHandlerServiceType);

        if (messageHandlerService is not IMessageHandler<IMessage> messageHandler)
            throw new InvalidOperationException("Message handler not found");

        return messageHandler;
    }
}
