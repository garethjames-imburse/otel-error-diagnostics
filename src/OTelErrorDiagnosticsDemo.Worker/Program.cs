using System.Reflection;
using Azure.Messaging.ServiceBus;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OTelErrorDiagnosticsDemo.Contract.Commands;
using OTelErrorDiagnosticsDemo.Worker.HostedServices;
using OTelErrorDiagnosticsDemo.Worker.Messaging;
using Serilog;

namespace OTelErrorDiagnosticsDemo.Worker;

public class Program
{
    public static void Main(string[] args)
    {
        Console.Title = Assembly.GetEntryAssembly()!.GetName()!.Name!;

        var builder = Host.CreateApplicationBuilder(args);

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
                    .AddOtlpExporter();
            });

        builder.Services.AddHostedService<QueueProcessorService>();
        builder.Services.AddSingleton<MessageConsumer>();
        builder.Services.AddScoped<IMessageHandler<MyCommand>, MyCommandHandler>();

        builder.Services.AddSingleton(serviceBusProvider => new ServiceBusClient(
            builder.Configuration.GetValue<string>("ServiceBus:ConnectionString")));

        var host = builder.Build();

        host.Run();
    }
}