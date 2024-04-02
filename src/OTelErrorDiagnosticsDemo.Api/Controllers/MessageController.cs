using Microsoft.AspNetCore.Mvc;
using OTelErrorDiagnosticsDemo.Contract.Commands;

namespace OTelErrorDiagnosticsDemo.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class MessageController : ControllerBase
{
    private readonly ILogger<MessageController> _logger;
    private readonly MessagePublisher _messagePublisher;

    public MessageController(
        ILogger<MessageController> logger,
        MessagePublisher messagePublisher)
    {
        _logger = logger;
        _messagePublisher = messagePublisher;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] MyCommand myCommand, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received command: {Command}", myCommand.Data);

        await _messagePublisher.PublishMessageAsync(myCommand, cancellationToken);
        return Accepted();
    }
}
