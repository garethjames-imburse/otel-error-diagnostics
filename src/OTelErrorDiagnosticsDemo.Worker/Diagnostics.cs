using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using OpenTelemetry.Resources;

namespace OTelErrorDiagnosticsDemo.Worker;

internal static partial class Diagnostics
{
    internal static readonly ActivitySource MyActivitySource = new("OTelErrorDiagnosticsDemo.Worker");

    internal static ResourceBuilder AddGitAttributes(this ResourceBuilder resourceBuilder)
    {
        var informationalVersion = typeof(Program).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? string.Empty;

        var sha = VersionRegex().Match(informationalVersion)
            .Groups["sha"]
            .Value;

        var gitAttributes = new Dictionary<string, object>(2)
        {
            { "git.commit.sha", sha },
            { "git.repository_url", "https://github.com/garethjames-imburse/otel-error-diagnostics" },
        };

        resourceBuilder.AddAttributes(gitAttributes);

        return resourceBuilder;
    }

    [GeneratedRegex(@"\d+\.\d+\.\d+\+(?<sha>.*)")]
    private static partial Regex VersionRegex();
}
