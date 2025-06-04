using System.ComponentModel;
using DotNetMetadataMcpServer.Configuration;
using DotNetMetadataMcpServer.Models;
using DotNetMetadataMcpServer.Services;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace DotNetMetadataMcpServer.ToolHandlers;

[McpServerToolType]
public static class AssemblyToolHandler
{
    [Description("Retrieves referenced assemblies based on filters and pagination (doesn't extract data from referenced projects. Notice that the project must be built before scanning.")]
    [McpServerTool(Name = "AssemblyToolHandler")]
    public static string HandleAsync(
        [Description("The assembly parameters to use for the request")] AssemblyToolParameters parameters,
        IServiceProvider serviceProvider)
    {
        var toolsConfiguration = serviceProvider.GetRequiredService<IOptions<ToolsConfiguration>>();
        var assemblyToolService = serviceProvider.GetRequiredService<AssemblyToolService>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(AssemblyToolHandler));
        logger.LogInformation("Received request to retrieve assemblies list with {Parameters}", parameters);

        AssemblyToolResponse result;
        try
        {
            result = assemblyToolService.GetAssemblies(
                projectFileAbsolutePath: parameters.ProjectFileAbsolutePath,
                filters: parameters.FullTextFiltersWithWildCardSupport,
                pageNumber: parameters.PageNumber,
                pageSize: toolsConfiguration.Value.DefaultPageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error scanning project {Path}", parameters.ProjectFileAbsolutePath);
            throw;
        }

        return toolsConfiguration.Value.Serialize(result);
    }
}