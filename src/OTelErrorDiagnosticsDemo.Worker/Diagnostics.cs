using System.Diagnostics;

namespace OTelErrorDiagnosticsDemo.Worker;

internal static class Diagnostics
{
    internal static readonly ActivitySource MyActivitySource = new("OTelErrorDiagnosticsDemo.Worker");
}
