using System.ComponentModel;
using DotNetMetadataMcpServer.Configuration;
using DotNetMetadataMcpServer.Models;
using DotNetMetadataMcpServer.Services;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace DotNetMetadataMcpServer.ToolHandlers;

[McpServerToolType]
public static class TypeToolHandler
{
    [Description("Retrieves types from specified namespaces supporting filters and pagination.")]
    [McpServerTool(Name = "TypeToolHandler")]
    public static string HandleAsync(
        [Description("The type tool parameters to use for the request")] TypeToolParameters parameters,
        IServiceProvider serviceProvider)
    {
        var toolsConfiguration = serviceProvider.GetRequiredService<IOptions<ToolsConfiguration>>();
        var typeToolService = serviceProvider.GetRequiredService<TypeToolService>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(TypeToolHandler));
        logger.LogInformation("Received request to retrieve types with {Parameters}", parameters);

        TypeToolResponse result;
        try
        {
            result = typeToolService.GetTypes(
                projectFileAbsolutePath: parameters.ProjectFileAbsolutePath,
                allowedNamespaces: parameters.Namespaces.ToList(),
                filters: parameters.FullTextFiltersWithWildCardSupport,
                pageNumber: parameters.PageNumber,
                pageSize: toolsConfiguration.Value.DefaultPageSize
            );
            logger.LogDebug("Types retrieved successfully: {@TypeToolResponse}", result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving types for project {Path}", parameters.ProjectFileAbsolutePath);
            throw;
        }

        return toolsConfiguration.Value.Serialize(result);
    }
}