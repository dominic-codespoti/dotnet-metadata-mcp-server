# Use SDK image for build and reflection
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY src/MetadataMCP.Server.csproj ./
RUN dotnet restore MetadataMCP.Server.csproj

# Copy the rest of the source code
COPY src/. ./

# Publish the application (self-contained, ready for runtime)
RUN dotnet publish MetadataMCP.Server.csproj -c Release -o /app/publish

# Use runtime image for final container
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "MetadataMCP.Server.dll"]
