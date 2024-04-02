FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

RUN apk add --no-cache icu-libs tzdata
RUN addgroup -g 1000 -S demo && \
    adduser -u 1000 -S demo -G demo

ENV ASPNETCORE_URLS=http://*:8080
COPY ["/output/OTelErrorDiagnosticsDemo.Api/", "."]

RUN chown -R demo:demo /app
USER demo

ENTRYPOINT ["dotnet", "OTelErrorDiagnosticsDemo.Api.dll"]