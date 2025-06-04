using System.ComponentModel;
using DotNetMetadataMcpServer.Configuration;
using DotNetMetadataMcpServer.Models;
using DotNetMetadataMcpServer.Services;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace DotNetMetadataMcpServer.ToolHandlers;

public static class NuGetToolHandlers
{
    [McpServerTool, Description("Searches for NuGet packages on nuget.org with support for filtering and pagination.")]
    public static async Task<string> NuGetPackageSearchHandleAsync(
        [Description("The NuGet package search parameters")] NuGetPackageSearchParameters parameters,
        IOptions<ToolsConfiguration> toolsConfiguration,
        NuGetToolService nuGetToolService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("NuGetPackageSearchToolHandler");
        logger.LogInformation("Received request to search NuGet packages with {Parameters}", parameters);

        NuGetPackageSearchResponse result;
        try
        {
            result = await nuGetToolService.SearchPackagesAsync(
                searchQuery: parameters.SearchQuery,
                filters: parameters.FullTextFiltersWithWildCardSupport,
                includePrerelease: parameters.IncludePrerelease,
                pageNumber: parameters.PageNumber,
                pageSize: toolsConfiguration.Value.DefaultPageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching NuGet packages with query {Query}", parameters.SearchQuery);
            throw;
        }

        return toolsConfiguration.Value.Serialize(result);
    }

    [McpServerTool, Description("Retrieves version history and dependency information for a specific NuGet package.")]
    public static async Task<string> NuGetPackageVersionsHandleAsync(
        [Description("The NuGet package version parameters")] NuGetPackageVersionParameters parameters,
        IOptions<ToolsConfiguration> toolsConfiguration,
        NuGetToolService nuGetToolService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("NuGetPackageVersionsToolHandler");
        logger.LogInformation("Received request to get NuGet package versions with {Parameters}", parameters);

        NuGetPackageVersionsResponse result;
        try
        {
            result = await nuGetToolService.GetPackageVersionsAsync(
                packageId: parameters.PackageId,
                filters: parameters.FullTextFiltersWithWildCardSupport,
                includePrerelease: parameters.IncludePrerelease,
                pageNumber: parameters.PageNumber,
                pageSize: toolsConfiguration.Value.DefaultPageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting versions for NuGet package {PackageId}", parameters.PackageId);
            throw;
        }

        return toolsConfiguration.Value.Serialize(result);
    }
}