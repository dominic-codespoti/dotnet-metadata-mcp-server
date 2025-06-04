using DotNetMetadataMcpServer;
using DotNetMetadataMcpServer.Configuration;
using DotNetMetadataMcpServer.Services;
using DotNetMetadataMcpServer.ToolHandlers;
using ModelContextProtocol.NET.Core.Models.Protocol.Common;
using ModelContextProtocol.NET.Server.Builder;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConfiguration(configuration.GetSection("Logging"))
        .AddConsole();
});

var logger = loggerFactory.CreateLogger("DotNetMetadataMcpServer");

try
{
    logger.LogInformation("Starting the server");

    var serverInfo = new Implementation
    {
        Name = "DotNet Projects Types Explorer MCP Server",
        Version = "1.0.0"
    };

    var builder = new McpServerBuilder(serverInfo).AddStdioTransport();
    builder.Services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
        loggingBuilder.AddConsole();
    });

    builder.Services.Configure<ToolsConfiguration>(configuration.GetSection(ToolsConfiguration.SectionName));

    builder.Services.AddScoped<MsBuildHelper>();
    builder.Services.AddScoped<ReflectionTypesCollector>();
    builder.Services.AddScoped(typeof(IDependenciesScanner), typeof(DependenciesScanner));

    builder.Services.AddScoped<AssemblyToolService>();
    builder.Services.AddScoped<NamespaceToolService>();
    builder.Services.AddScoped<TypeToolService>();
    builder.Services.AddScoped<NuGetToolService>();

    builder.Tools.AddHandler<AssemblyToolHandler>();
    builder.Tools.AddHandler<NamespaceToolHandler>();
    builder.Tools.AddHandler<TypeToolHandler>();
    builder.Tools.AddHandler<NuGetPackageSearchToolHandler>();
    builder.Tools.AddHandler<NuGetPackageVersionsToolHandler>();

    var host = builder.Build();
    host.Start();
    await Task.Delay(-1);
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred while running the server");
    Console.WriteLine(ex);
    Environment.Exit(1);
}
finally
{
    logger.LogInformation("Shutting down the server");
}
