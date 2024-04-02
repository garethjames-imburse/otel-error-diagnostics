using Azure.Messaging.ServiceBus;
using OTelErrorDiagnosticsDemo.Worker.Messaging;

namespace OTelErrorDiagnosticsDemo.Worker.HostedServices;

public sealed class QueueProcessorService : IHostedService, IAsyncDisposable
{
    private readonly ILogger<QueueProcessorService> _logger;
    private readonly MessageConsumer _messageConsumer;
    private readonly ServiceBusProcessor _processor;

    public QueueProcessorService(
        ILogger<QueueProcessorService> logger,
        MessageConsumer messageConsumer,
        ServiceBusClient serviceBusClient)
    {
        _logger = logger;
        _messageConsumer = messageConsumer;
        _processor = serviceBusClient.CreateProcessor("otelerrordiagnosticsdemo-queue");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _processor.ProcessMessageAsync += Processor_ProcessMessageAsync;
        _processor.ProcessErrorAsync += Processor_ProcessErrorAsync;

        await _processor.StartProcessingAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);

        _processor.ProcessMessageAsync -= Processor_ProcessMessageAsync;
        _processor.ProcessErrorAsync -= Processor_ProcessErrorAsync;

        await _processor.CloseAsync(cancellationToken);
    }

    public ValueTask DisposeAsync() => _processor.DisposeAsync();

    private async Task Processor_ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        await _messageConsumer.ConsumeMessageAsync(args);
    }

    private Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        using var _ = _logger.BeginScope(new Dictionary<string, object>
        {
            { nameof(arg.EntityPath), arg.EntityPath },
            { nameof(arg.ErrorSource), arg.ErrorSource },
            { nameof(arg.Identifier), arg.Identifier }
        });

        _logger.LogError(arg.Exception, "An error occurred");

        return Task.CompletedTask;
    }
}
