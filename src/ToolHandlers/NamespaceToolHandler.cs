using System.ComponentModel;
using DotNetMetadataMcpServer.Configuration;
using DotNetMetadataMcpServer.Models;
using DotNetMetadataMcpServer.Services;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace DotNetMetadataMcpServer.ToolHandlers;

public static class NamespaceToolHandler
{
    [McpServerTool, Description("Retrieves namespaces from specified assemblies supporting filters and pagination (doesn't extract data from referenced projects. Notice that the project must be built before scanning.")]
    public static string HandleAsync(
        [Description("The namespace parameters to use for the request")] NamespaceToolParameters parameters,
        IOptions<ToolsConfiguration> toolsConfiguration,
        NamespaceToolService namespaceToolService,
        ILoggerFactory loggerFactory)
    {
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