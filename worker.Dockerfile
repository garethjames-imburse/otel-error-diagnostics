FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

COPY ["/output/OTelErrorDiagnosticsDemo.Worker/", "."]

ENTRYPOINT ["dotnet", "OTelErrorDiagnosticsDemo.Worker.dll"]