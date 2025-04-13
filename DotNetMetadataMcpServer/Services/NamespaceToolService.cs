using System.ComponentModel;
using DotNetMetadataMcpServer.Helpers;
using DotNetMetadataMcpServer.Models;
using ModelContextProtocol.Server;

namespace DotNetMetadataMcpServer.Services
{
    [McpServerToolType]
    public class NamespaceToolService
    {
        private readonly IDependenciesScanner _scanner;
        private readonly ILogger<NamespaceToolService> _logger;

        public NamespaceToolService(IDependenciesScanner scanner, ILogger<NamespaceToolService> logger)
        {
            _scanner = scanner;
            _logger = logger;
        }

        [McpServerTool(Name = "NamespacesExplorer")] 
        [Description("Retrieves namespaces from specified assemblies supporting filters and pagination (doesn't extract data from referenced projects. " +
                     "Notice that the project must be built before scanning.")]
        public NamespaceToolResponse GetNamespaces(
            [Description("TODO")] string projectFileAbsolutePath,
            [Description("TODO")] List<string> allowedAssemblyNames, 
            [Description("TODO")] List<string> filters, 
            [Description("TODO")] int pageNumber, 
            [Description("TODO")] int pageSize)
        {
            using var _ = _logger.BeginScope("{NamespaceToolExecutionUid}", Guid.NewGuid());
            _logger.LogInformation("Received request to retrieve namespaces with {@Parameters}", new
            {
                ProjectFileAbsolutePath = projectFileAbsolutePath,
                AllowedAssemblyNames = allowedAssemblyNames,
                Filters = filters,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
            
            try
            {
                var metadata = _scanner.ScanProject(projectFileAbsolutePath);

                var allowedAssemblyNamesWithoutExtension = allowedAssemblyNames
                    .Select(Path.GetFileNameWithoutExtension)
                    .Where(s => s != null)
                    .Select(s => s!.ToLowerInvariant())
                    .ToHashSet();

                // Build the allowed namespaces only from types of assemblies matching allowedAssemblyNames.
                var allowedNamespaces = new List<string>();
                if (allowedAssemblyNames is { Count: > 0 })
                {
                    // Include namespaces from the main project if its assembly name is allowed.
                    var mainAssemblyNameWithoutExtension = Path.GetFileNameWithoutExtension(metadata.AssemblyPath);
                    if (allowedAssemblyNamesWithoutExtension.Contains(mainAssemblyNameWithoutExtension.ToLowerInvariant()))
                    {
                        allowedNamespaces.AddRange(ExtractNamespaces(metadata.ProjectTypes));
                    }

                    // Include namespaces from dependencies whose Name is in allowedAssemblyNames.
                    foreach (var dep in metadata.Dependencies)
                    {
                        var depNameWithoutExtension = Path.GetFileNameWithoutExtension(dep.Name);
                        if (allowedAssemblyNamesWithoutExtension.Contains(depNameWithoutExtension.ToLowerInvariant()))
                        {
                            allowedNamespaces.AddRange(ExtractNamespaces(dep.Types));
                        }
                    }
                }
                else
                {
                    allowedNamespaces.AddRange(ExtractNamespaces(metadata.ProjectTypes));

                    foreach (var dep in metadata.Dependencies)
                    {
                        allowedNamespaces.AddRange(ExtractNamespaces(dep.Types));
                    }
                }

                // Remove duplicates.
                var allNamespaces = allowedNamespaces.Distinct();

                // Apply additional filter if provided.
                if (filters.Any())
                {
                    var predicates = filters.Select(FilteringHelper.PrepareFilteringPredicate).ToList();
                    allNamespaces = allNamespaces.Where(n => predicates.Any(predicate => predicate.Invoke(n)));
                }

                // Paginate the namespaces.
                var (paged, availablePages) = PaginationHelper.FilterAndPaginate(allNamespaces, _ => true, pageNumber, pageSize);
                return new NamespaceToolResponse
                {
                    Namespaces = paged,
                    CurrentPage = pageNumber,
                    AvailablePages = availablePages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving namespaces");
                throw;
            }
        }

        private static IEnumerable<string> ExtractNamespaces(IEnumerable<TypeInfoModel> types)
        {
            return types
                .Where(t => !string.IsNullOrWhiteSpace(t.FullName) && t.FullName.Contains('.'))
                .Select(t => t.FullName.Substring(0, t.FullName.LastIndexOf('.')));
        }
    }
}