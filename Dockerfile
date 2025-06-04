FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

COPY src/bin/Release/net9.0/ ./

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "DotNetMetadataMcpServer.dll"]
