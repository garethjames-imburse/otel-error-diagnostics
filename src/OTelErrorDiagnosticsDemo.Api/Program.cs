using System.Reflection;
using Azure.Messaging.ServiceBus;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace OTelErrorDiagnosticsDemo.Api;

public class Program
{
    public static void Main(string[] args)
    {
        Console.Title = Assembly.GetEntryAssembly()!.GetName()!.Name!;

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddSerilog((serviceProvider, loggerConfguration) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            loggerConfguration.ReadFrom.Configuration(configuration);
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(configure => configure.AddEnvironmentVariableDetector())
            .WithTracing(builder =>
            {
                builder
                    .AddSource(
                        "Azure.Messaging.ServiceBus.ServiceBusSender",
                        "Azure.Messaging.ServiceBus.ServiceBusProcessor",
                        "OTelErrorDiagnosticsDemo.*")
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter();
            });

        builder.Services.AddSingleton(serviceBusProvider => new ServiceBusClient(
            builder.Configuration.GetValue<string>("ServiceBus:ConnectionString")));

        builder.Services.AddSingleton<MessagePublisher>();

        var app = builder.Build();

        app.MapControllers();

        app.Run();
    }
}
