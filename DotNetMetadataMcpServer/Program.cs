using System.Text.Json;
using DotNetMetadataMcpServer.Configuration;
using DotNetMetadataMcpServer.ConfigurationExtensions;
using DotNetMetadataMcpServer.Services;
using ModelContextProtocol.Protocol.Types;
using Serilog;

namespace DotNetMetadataMcpServer;

// ReSharper disable once UnusedType.Global
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class Program
{
    /*/// <summary>
    /// DotNet Metadata MCP Server
    /// </summary>
    /// <param name="homeEnvVariable">The home environment variable</param>
    /// <returns></returns>*/
    public static async Task<int> Main(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[0]) || args[0] != "--homeEnvVariable" || string.IsNullOrWhiteSpace(args[1]))
        {
            Console.WriteLine("The --homeEnvVariable argument with a value is required");
            return 1;
        } 
        
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        
        var homeEnvVariable = args[1];
        Environment.SetEnvironmentVariable("HOME", homeEnvVariable);
        
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("RunId", Guid.NewGuid())
            .CreateLogger();
        
        Log.Logger = logger; 
        
        try
        {
            logger.Information("Starting the server");

            var toolsConfiguration = configuration.GetSection(ToolsConfiguration.SectionName).Get<ToolsConfiguration>();
            if (toolsConfiguration == null)
            {
                logger.Error("Tools configuration is missing");
                return 1;
            }
            
            
            var builder = Host.CreateApplicationBuilder();

            
            var serverInfo = new Implementation
            {
                Name = "DotNet Projects Types Explorer MCP Server",
                Version = "1.0.0"
            };
            var mcpBuilder = builder.Services.AddMcpServer(options =>
            {
                options.ServerInfo = serverInfo;
            });
            mcpBuilder.WithStdioServerTransport();
            
            var jsonSerializerOptions = toolsConfiguration.IntendResponse 
                ? new JsonSerializerOptions { WriteIndented = true } 
                : new JsonSerializerOptions { WriteIndented = false };

            mcpBuilder.WithScopedTools(
            [
                typeof(AssemblyToolService),
                typeof(NamespaceToolService),
                typeof(TypeToolService),
                typeof(NuGetToolService)
            ], jsonSerializerOptions);
            
            
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));
            
            builder.Services.Configure<ToolsConfiguration>(configuration.GetSection(ToolsConfiguration.SectionName));
            
            builder.Services.AddScoped<MsBuildHelper>();
            builder.Services.AddScoped<ReflectionTypesCollector>();
            builder.Services.AddScoped(typeof(IDependenciesScanner), typeof(DependenciesScanner));

            builder.Services.AddScoped<AssemblyToolService>();
            builder.Services.AddScoped<NamespaceToolService>();
            builder.Services.AddScoped<TypeToolService>();
            builder.Services.AddScoped<NuGetToolService>();
            
            
            await builder.Build().RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An error occurred while running the server");
            Console.WriteLine(ex);
            
            return 1;
        }
        finally
        {
            logger.Information("Shutting down the server");
            await Log.CloseAndFlushAsync();
        }
    }
}
