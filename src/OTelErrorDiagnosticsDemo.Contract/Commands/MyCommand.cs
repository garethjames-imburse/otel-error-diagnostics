namespace OTelErrorDiagnosticsDemo.Contract.Commands;

public class MyCommand : IMessage
{
    public required object Data { get; set; }
}
