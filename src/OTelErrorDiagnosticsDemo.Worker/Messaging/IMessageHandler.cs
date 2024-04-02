using OTelErrorDiagnosticsDemo.Contract;

namespace OTelErrorDiagnosticsDemo.Worker.Messaging;

public interface IMessageHandler<out TMessage> where TMessage : IMessage
{
    Task HandleMessageAsync(IMessage message, CancellationToken cancellationToken);
}
