using OTelErrorDiagnosticsDemo.Contract;
using OTelErrorDiagnosticsDemo.Contract.Commands;

namespace OTelErrorDiagnosticsDemo.Worker.Messaging;

public class MyCommandHandler : IMessageHandler<MyCommand>
{
    private readonly ILogger<MyCommandHandler> _logger;

    public MyCommandHandler(ILogger<MyCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleMessageAsync(IMessage message, CancellationToken cancellationToken)
    {
        var myCommand = (MyCommand)message;

        _logger.LogInformation("Handled command with data: {Data}", myCommand.Data);

        return Task.CompletedTask;
    }
}
