Remove-Item .\output\OTelErrorDiagnosticsDemo.Worker -Recurse -Force -ErrorAction Ignore

#dotnet publish .\src\OTelErrorDiagnosticsDemo.Api\ -o .\output\OTelErrorDiagnosticsDemo.Api -c Release --interactive
dotnet publish .\src\OTelErrorDiagnosticsDemo.Worker\ -o .\output\OTelErrorDiagnosticsDemo.Worker -c Release --interactive

docker-compose --env-file .\.env up --build -d