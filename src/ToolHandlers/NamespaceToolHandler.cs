using System.ComponentModel;
using DotNetMetadataMcpServer.Configuration;
using DotNetMetadataMcpServer.Models;
using DotNetMetadataMcpServer.Services;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace DotNetMetadataMcpServer.ToolHandlers;

[McpServerToolType]
public static class NamespaceToolHandler
{
    [Description("Retrieves namespaces from specified assemblies supporting filters and pagination (doesn't extract data from referenced projects. Notice that the project must be built before scanning.")]
    [McpServerTool(Name = "NamespaceToolHandler")]
    public static string HandleAsync(
        [Description("The namespace parameters to use for the request")] NamespaceToolParameters parameters,
        IServiceProvider serviceProvider)
    {
        var toolsConfiguration = serviceProvider.GetRequiredService<IOptions<ToolsConfiguration>>();
        var namespaceToolService = serviceProvider.GetRequiredService<NamespaceToolService>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(NamespaceToolHandler));
        logger.LogInformation("Received request to retrieve namespaces with {Parameters}", parameters);

        NamespaceToolResponse result;
        try
        {
            result = namespaceToolService.GetNamespaces(
                projectFileAbsolutePath: parameters.ProjectFileAbsolutePath,
                allowedAssemblyNames: parameters.AssemblyNames.ToList(),
                filters: parameters.FullTextFiltersWithWildCardSupport,
                pageNumber: parameters.PageNumber,
                pageSize: toolsConfiguration.Value.DefaultPageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving namespaces for project {Path}", parameters.ProjectFileAbsolutePath);
            throw;
        }

        return toolsConfiguration.Value.Serialize(result);
    }
}